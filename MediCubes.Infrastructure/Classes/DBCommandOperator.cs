using System.Data;
using Application.Classes;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infrastructure.Classes
{
    public class DBCommandOperator:IDBCommandOperator
    {
        private readonly AppDbContext _context;
        private readonly EnumAppSetting.AppSetting _dbType;

        public DBCommandOperator(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _dbType = (EnumAppSetting.AppSetting)config.GetValue<int>("DatabaseSettings:Database");
        }

        public async Task<List<T>> ExecuteQueryAsync<T>(string procOrFuncName, Dictionary<string, object>? parameters = null) where T : class
        {
            var sql = BuildQuery(procOrFuncName, parameters);
            var dbParams = BuildParameters(parameters);
            return await _context.Set<T>().FromSqlRaw(sql, dbParams.ToArray()).ToListAsync();
        }

        public async Task<int> ExecuteNonQueryAsync(string procOrFuncName, Dictionary<string, object>? parameters = null)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = BuildQuery(procOrFuncName, parameters);
            foreach (var p in BuildParameters(parameters))
                cmd.Parameters.Add(p);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string procOrFuncName, Dictionary<string, object>? parameters = null)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = BuildQuery(procOrFuncName, parameters);
            foreach (var p in BuildParameters(parameters))
                cmd.Parameters.Add(p);

            return await cmd.ExecuteScalarAsync();
        }

        public async Task<DataTable> ExecuteDataTableAsync(string procOrFuncName, Dictionary<string, object>? parameters = null)
        {
            using var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = BuildQuery(procOrFuncName, parameters);
            foreach (var p in BuildParameters(parameters))
                cmd.Parameters.Add(p);

            var dt = new DataTable();

            if (_dbType == EnumAppSetting.AppSetting.SQL)
            {
                using var adapter = new SqlDataAdapter((SqlCommand)cmd);
                adapter.Fill(dt);
            }
            else
            {
                using var adapter = new NpgsqlDataAdapter((NpgsqlCommand)cmd);
                adapter.Fill(dt);
            }

            return dt;
        }

        private string BuildQuery(string name, Dictionary<string, object>? parameters)
        {
            var paramNames = parameters != null && parameters.Any()
                ? string.Join(", ", parameters.Select(p => "@" + p.Key))
                : string.Empty;

            return _dbType switch
            {
                EnumAppSetting.AppSetting.SQL => $"EXEC {name} {paramNames}",
                EnumAppSetting.AppSetting.PostGre => $"SELECT * FROM {name}({paramNames})",
                _ => throw new NotSupportedException("Unsupported database type")
            };
        }

        private List<IDataParameter> BuildParameters(Dictionary<string, object>? parameters)
        {
            var list = new List<IDataParameter>();
            if (parameters == null) return list;

            foreach (var (key, value) in parameters)
            {
                if (_dbType == EnumAppSetting.AppSetting.SQL)
                    list.Add(new SqlParameter("@" + key, value ?? DBNull.Value));
                else
                    list.Add(new NpgsqlParameter("@" + key, value ?? DBNull.Value));
            }
            return list;
        }
    }
}
