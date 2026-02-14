using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Infrastructure.Repositories.JWT;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoleService : IRoleService
    {
        private readonly IJWTService _jWTService;
        private readonly AppDbContext _db;

        public RoleService(IJWTService jWTService, AppDbContext db)
        {
            _jWTService = jWTService;
            _db = db;
        }
        public async Task<IEnumerable<MenuDTO>> GetAllMenusListForCreatingRoles()
        {
            var jwtClaims = _jWTService.DecodeToken();


            var allMenus = await _db.AppMenus.Where(menu => menu.IsDeleted == 0 && menu.TenantId == jwtClaims.TenantId).AsNoTracking().ToListAsync();

            var menuLookup = allMenus.ToLookup(m => m.ParentId);
            var rootMenus = menuLookup[null]; // Root menus have no ParentId

            // Pass the access permissions into the hierarchy builder
            return BuildMenuHierarchy(rootMenus, menuLookup, null);
        }
        private IEnumerable<MenuDTO> BuildMenuHierarchy(
            IEnumerable<AppMenu> rootMenus,
            ILookup<int?, AppMenu> menuLookup,
            string parentMenuCode)
        {
            var sortedMenus = rootMenus.OrderBy(m => m.Order);
            foreach (var menu in sortedMenus)
            {
                var currentMenuCode = string.IsNullOrEmpty(parentMenuCode) ? menu.Title.ToLower() : $"{parentMenuCode}.{menu.Title.ToLower()}";

                var menuDto = new MenuDTO
                {
                    MenuId = menu.Id,
                    Id = currentMenuCode,
                    Title = menu.Title,
                    Subtitle = menu.Subtitle,
                    Type = menu.Type.ToString(),
                    Icon = menu.Icon,
                    Link = menu.Link,
                    ParentId = menu.ParentId,
                    Children = BuildMenuHierarchy(menuLookup[menu.Id], menuLookup, currentMenuCode).ToList()
                };
                yield return menuDto;
            }
        }
        ////////////////////////GetAllMenusListForCreatingRoles----End-----//////////////////////////////////




        public async Task<object> CreateRole(CreateOrUpdateRoleDTO createRoleRequest)
        {
            var appRole = createRoleRequest.RoleName;
            var menus = createRoleRequest.Menus;


            //must change later
            var jwtClaims = _jWTService.DecodeToken();

            var existingRole = await _db.Roles.FirstOrDefaultAsync(role => role.Name == appRole);

            if (existingRole != null)
            {
                return new
                {
                    message = "Role already exists!",
                    IsSucceeded = 0,
                    role = existingRole
                };
            }

            var role = new AppRole
            {
                Name = appRole,
                NormalizedName = appRole.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.UtcNow,
                CreatedBy = jwtClaims.UserId ,
                TenantId = jwtClaims.UserId,
                IsDeleted = 0
            };

            await _db.Roles.AddAsync(role);
            await _db.SaveChangesAsync();

            var roleMenus = menus.Select(menu => new AppRoleAppMenu
            {
                RoleId = role.Id,
                MenuId = menu.MenuId,
                CreateAccess = menu.CreateAccess,
                DeleteAccess = menu.DeleteAccess,
                EditAccess = menu.EditAccess,
                PrintAccess = menu.PrintAccess
            }).ToList();

            await _db.BulkInsertAsync(roleMenus);

            return new
            {
                message = "Role and associated menus added successfully!",
                IsSucceeded = 1
            };


        }

        public async Task<object> UpdateRole(CreateOrUpdateRoleDTO updateRoleRequest)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // Decode the JWT token to get the tenant ID
                    //var jwtClaims = await jWTService.DecodeJWTToken();

                    // Extract request data
                    var roleId = updateRoleRequest.RoleId;
                    var roleName = updateRoleRequest.RoleName;
                    var menus = updateRoleRequest.Menus;

                    // Find the existing role
                    var existingRole = await _db.Roles.SingleOrDefaultAsync(role => role.Id == roleId && role.TenantId == 1); // Adjust TenantId logic as needed
                    if (existingRole == null)
                    {
                        return new
                        {
                            message = "Role not found!",
                            IsSucceeded = 0
                        };
                    }

                    // Update role properties
                    existingRole.Name = roleName;
                    existingRole.NormalizedName = roleName.ToUpperInvariant();
                    existingRole.ModifiedOn = DateTime.UtcNow;
                    existingRole.ModifiedBy = 1; // Replace with jwtClaims.Id or appropriate user identifier

                    _db.Roles.Update(existingRole);
                    await _db.SaveChangesAsync();

                    // Get existing role-menu mappings
                    var existingRoleMenus = await _db.RoleMenus.Where(rm => rm.RoleId == existingRole.Id).ToListAsync();

                    // Remove old mappings
                    _db.RoleMenus.RemoveRange(existingRoleMenus);
                    await _db.SaveChangesAsync();

                    // Add new menu permissions
                    var newRoleMenus = menus.Select(menu => new AppRoleAppMenu
                    {
                        RoleId = existingRole.Id,
                        MenuId = menu.MenuId,
                        CreateAccess = menu.CreateAccess,
                        DeleteAccess = menu.DeleteAccess,
                        EditAccess = menu.EditAccess,
                        PrintAccess = menu.PrintAccess
                    }).ToList();

                    await _db.BulkInsertAsync(newRoleMenus);
                    await _db.SaveChangesAsync();

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return new
                    {
                        message = "Role and associated menus updated successfully!",
                        IsSucceeded = 1
                    };
                }
                catch (Exception ex)
                {
                    // Rollback the transaction on error
                    await transaction.RollbackAsync();

                    return new
                    {
                        message = $"An error occurred: {ex.Message}",
                        IsSucceeded = 0
                    };
                }
            }
        }


        public async Task<object> GetRolesList()
        {
            var allRoles = await _db.Roles.Where(r => r.IsDeleted == 0).AsNoTracking().ToListAsync();
            return allRoles;
        }

        public async Task<object> GetRoleById(int RoleId)
        {
            var jwtClaims = _jWTService.DecodeToken();

            var menus = await (from roleMenus in _db.RoleMenus
                               where roleMenus.RoleId == RoleId
                               select new
                               {
                                   roleMenus.MenuId,
                                   roleMenus.CreateAccess,
                                   roleMenus.DeleteAccess,
                                   roleMenus.EditAccess,
                                   roleMenus.PrintAccess
                               }).ToListAsync();

            var role = await (from r in _db.Roles.Where(x => x.Id == RoleId && x.IsDeleted == 0 && x.TenantId == jwtClaims.TenantId)
                              select new
                              {
                                  RoleName = r.Name
                              }
                               ).SingleOrDefaultAsync();
            return new
            {
                role,
                menus
            };
        }
        public async Task<object> DeleteRoleById(int RoleId)
        {
            var jwtClaims = _jWTService.DecodeToken();
            var existingRole = await _db.Roles.SingleOrDefaultAsync(role => role.Id == RoleId && role.IsDeleted == 0 && role.TenantId == jwtClaims.TenantId);
            if (existingRole == null)
            {
                return new { Success = false, Message = "Role not found" };
            }
            existingRole.IsDeleted = 1;
            existingRole.ModifiedBy = jwtClaims.UserId;
            existingRole.ModifiedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return new
            {
                IsSucceeded = 1,
                Message = "Role deleted successfully!"
            };
        }
    }
}
