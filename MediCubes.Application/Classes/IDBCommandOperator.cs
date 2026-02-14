using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Classes
{
    public interface IDBCommandOperator
    {
        Task<List<T>> ExecuteQueryAsync<T>(string procOrFuncName, Dictionary<string, object>? parameters = null) where T : class;
        Task<int> ExecuteNonQueryAsync(string procOrFuncName, Dictionary<string, object>? parameters = null);
        Task<object?> ExecuteScalarAsync(string procOrFuncName, Dictionary<string, object>? parameters = null);
        Task<DataTable> ExecuteDataTableAsync(string procOrFuncName, Dictionary<string, object>? parameters = null);
    }
}
