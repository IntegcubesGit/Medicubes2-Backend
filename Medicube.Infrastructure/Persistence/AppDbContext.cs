using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserAppRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<IHSCode> IHSCodes { get; set; }
        public DbSet<ISaleType> ISaleTypes { get; set; }
        public DbSet<IUnitType> IUnitTypes { get; set; }
        public DbSet<IVendorType> IVendorTypes { get; set; }
        public DbSet<IStateType> IStateTypes { get; set; }
        public DbSet<ICustRegType> ICustRegTypes { get; set; }
        public DbSet<ICustomerType> ICustomerTypes { get; set; }
        public DbSet<SaleDelivery> SaleDeliveries { get; set; }
        public DbSet<SaleDeliveryDetail> SaleDeliveryDetails { get; set; }
        public DbSet<IDeliveryStatus> IDeliveryStatuses { get; set; }
        public DbSet<InvoiceStatus> InvoiceStatuses { get; set; }
        public DbSet<IFBRScenario> IFBRScenarios { get; set; }
        public DbSet<IReasonType> IReasonTypes { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<OrgAppSetting> OrgAppSettings { get; set; }
        public DbSet<OrgLocation> OrgLocations { get; set; }
        public DbSet<InvStoreStock> InvStoreStocks { get; set; }
        public DbSet<InvInventory> InvInventories { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<ICurrency> ICurrencies { get; set; }
        public DbSet<AppMenu> AppMenus { get; set; }
        public DbSet<AppUserStaff> AppUserStaff { get; set; }
        public DbSet<AppUserLocation> AppUserLocation { get; set; }
        public DbSet<ConfigSetting> ConfigSettings { get; set; }
        public DbSet<AppRoleAppMenu> RoleMenus { get; set; }
        public DbSet<OrgInfo> OrgInfos { get; set; }
        public DbSet<AccTaxChallan> AccTaxChallan { get; set; }
        public DbSet<AccTaxChallanDetail> AccTaxChallanDetail { get; set; }
        public DbSet<StockHistoryLogs> StockHistoryLogs { get; set; }
        public DbSet<IInvoiceType> IInvoiceTypes { get; set; }
        public DbSet<ITaxRate> ITaxRates { get; set; }
        public DbSet<ISroSchedule> ISroSchedules { get; set; }
        public DbSet<ISroItemCode> ISroItemCodes { get; set; }
        public DbSet<FBRDataFetchLog> FBRDataFetchLogs { get; set; }
        public DbSet<InvoiceProductLog> InvoiceProductLogs { get; set; }
        //public DbSet<AppMenuView> AppMenusView { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTime, DateTime>(
                                v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                    }
                }
            }

            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("appuser");
                entity.Property(u => u.Id).HasColumnName("userid");
            });

            builder.Entity<AppRole>(entity =>
            {
                entity.ToTable("AppRoles");
            });

            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("userclaims");
            });

            builder.Entity<AppUserAppRole>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("userlogins");
            });

            builder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.ToTable("roleclaims");
            });

            builder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("usertokens");
            });

            builder.Entity<AppRoleAppMenu>().HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();

            // Configure the AppMenusView as a read-only entity
            //builder.Entity<AppMenuView>(entity =>
            //{
            //    entity.HasNoKey(); // No primary key since it's a view
            //    entity.ToView("AppMenusView"); // Map the view to the model
            //});

        }
    }
}
