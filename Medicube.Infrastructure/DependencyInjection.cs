using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Domain.Enums;
using Application.Classes;
using Infrastructure.Classes;
using Application.Common.Interfaces.JWT;
using Infrastructure.Repositories.JWT;
using Application.Common.Interfaces.Auth;
using Application.Common.Interfaces.Products;
using Infrastructure.Repositories.Auth;
using Infrastructure.Repositories.Products;
using Application.Common.Interfaces.Customers;
using Infrastructure.Repositories.Customers;
using Application.Common.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Services;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            int IsCurrentDatabase = config.GetValue<int>("DatabaseSettings:Database");
            if (IsCurrentDatabase == (int)EnumAppSetting.AppSetting.PostGre)
            {
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(config.GetConnectionString("Postgres")));
                services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(config.GetConnectionString("AuthPostgres")));
            }
            if (IsCurrentDatabase == (int)EnumAppSetting.AppSetting.SQL)
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


            // Register Repositories
            services.AddSingleton<ITimeService, TimeService>();
            services.AddScoped<IProductService, ProductRepository>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IDBCommandOperator, DBCommandOperator>();
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<ICommonService, CommonService>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<ISaleDeliveryService, SaleDeliveryService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<ICreditNoteService, CreditNoteService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IOrgAppSettingService, OrgAppSettingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IOrgInfoService, OrgInfoService>();
            services.AddScoped<ITaxConsolidationService, TaxConsolidationService>();

            return services;
        }
    }
}
