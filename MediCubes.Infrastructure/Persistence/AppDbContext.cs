using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserAppRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        private readonly ICurrentUser? _currentUser;

        /// <summary>For design-time (migrations) and when tenant context is not available.</summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            _currentUser = null;
        }

        /// <summary>For runtime; enables global query filter by tenant.</summary>
        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<OrgAppSetting> OrgAppSettings { get; set; }
        public DbSet<OrgLocation> OrgLocations { get; set; }
        public DbSet<ICurrency> ICurrencies { get; set; }
        public DbSet<AppMenu> AppMenus { get; set; }
        public DbSet<AppUserStaff> AppUserStaff { get; set; }
        public DbSet<AppUserLocation> AppUserLocation { get; set; }
        public DbSet<ConfigSetting> ConfigSettings { get; set; }
        public DbSet<AppRoleAppMenu> RoleMenus { get; set; }
        public DbSet<OrgInfo> OrgInfos { get; set; }

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
                entity.ToTable("app_user");
                entity.Property(u => u.Id).HasColumnName("userid");
            });

            builder.Entity<AppRole>(entity =>
            {
                entity.ToTable("app_role");
            });

            builder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("userclaims");
            });

            builder.Entity<AppUserAppRole>(entity =>
            {
                entity.ToTable("app_userrole");
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

            builder.Entity<AppRoleAppMenu>(entity =>
            {
                entity.ToTable("app_rolemenu");
                entity.HasIndex(x => new { x.RoleId, x.MenuId }).IsUnique();
            });

            // Global query filter for tenant-scoped entities when ICurrentUser is available (e.g. not during migrations)
            if (_currentUser != null)
            {
                builder.Entity<OrgAppSetting>().HasQueryFilter(e => e.OrgId == _currentUser.GetCurrentUser().OrgId);
                builder.Entity<OrgLocation>().HasQueryFilter(e => e.OrgId == _currentUser.GetCurrentUser().OrgId);
                builder.Entity<AppMenu>().HasQueryFilter(e => e.OrgId == _currentUser.GetCurrentUser().OrgId);
            }
        }
    }
}
