using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Application.Common.Interfaces.JWT;
using Infrastructure.Repositories.JWT;
using Application.Common.Interfaces.Auth;
using Infrastructure.Repositories.Auth;
using Application.Common.Interfaces;
using Application.Classes;
using Infrastructure.Repositories;
using Infrastructure.Classes;
using Infrastructure.Services;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            int databaseType = config.GetValue<int>("DatabaseSettings:Database");
            if (databaseType == (int)Domain.Enums.EnumAppSetting.AppSetting.PostGre)
            {
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(config.GetConnectionString("Postgres")));
                services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(config.GetConnectionString("AuthPostgres")));
            }
            if (databaseType == (int)Domain.Enums.EnumAppSetting.AppSetting.SQL)
            {
                services.AddDbContext<AppDbContext>(options => options.UseSqlServer(config.GetConnectionString("SqlServer")));
                services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(config.GetConnectionString("AuthSqlServer")));
            }

            services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password = new PasswordOptions
                {
                    RequireDigit = true,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false,
                    RequiredLength = 3,
                    RequiredUniqueChars = 0
                };
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            services.AddSingleton<ITimeService, TimeService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDBCommandOperator, DBCommandOperator>();
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<ICurrentUser, CurrentUserService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IOrgAppSettingService, OrgAppSettingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOrgInfoService, OrgInfoService>();

            return services;
        }
    }
}
