using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using EFCore.BulkExtensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoleService : IRoleService
    {
        private readonly ICurrentUser _currentUser;
        private readonly AppDbContext _db;

        public RoleService(ICurrentUser currentUser, AppDbContext db)
        {
            _currentUser = currentUser;
            _db = db;
        }
        public async Task<IEnumerable<MenuDTO>> GetAllMenusListForCreatingRoles()
        {
            var jwtClaims = _currentUser.GetCurrentUser();

            var allMenus = await _db.AppMenus.Where(menu => menu.IsDeleted == 0 && menu.OrgId == jwtClaims.OrgId).AsNoTracking().ToListAsync();

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


            var jwtClaims = _currentUser.GetCurrentUser();

            var existingRole = await _db.Roles.FirstOrDefaultAsync(role => role.Name == appRole && role.OrgId == jwtClaims.OrgId);

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
                CreatedBy = jwtClaims.UserId,
                OrgId = jwtClaims.OrgId,
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
                    // Extract request data
                    var roleId = updateRoleRequest.RoleId;
                    var roleName = updateRoleRequest.RoleName;
                    var menus = updateRoleRequest.Menus;

                    var jwtClaims = _currentUser.GetCurrentUser();
                    var existingRole = await _db.Roles.SingleOrDefaultAsync(role => role.Id == roleId && role.OrgId == jwtClaims.OrgId && role.IsDeleted == 0);
                    if (existingRole == null)
                    {
                        return new
                        {
                            message = "Role not found!",
                            IsSucceeded = 0
                        };
                    }

                    existingRole.Name = roleName;
                    existingRole.NormalizedName = roleName.ToUpperInvariant();
                    existingRole.ModifiedOn = DateTime.UtcNow;
                    existingRole.ModifiedBy = jwtClaims.UserId;

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
            var currentUser = _currentUser.GetCurrentUser();
            var allRoles = await _db.Roles.Where(r => r.IsDeleted == 0 && r.OrgId == currentUser.OrgId).AsNoTracking().ToListAsync();
            return allRoles;
        }

        public async Task<object> GetRoleById(int RoleId)
        {
            var jwtClaims = _currentUser.GetCurrentUser();

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

            var role = await (from r in _db.Roles.Where(x => x.Id == RoleId && x.IsDeleted == 0 && x.OrgId == jwtClaims.OrgId)
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
            var jwtClaims = _currentUser.GetCurrentUser();
            var existingRole = await _db.Roles.SingleOrDefaultAsync(role => role.Id == RoleId && role.IsDeleted == 0 && role.OrgId == jwtClaims.OrgId);
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
