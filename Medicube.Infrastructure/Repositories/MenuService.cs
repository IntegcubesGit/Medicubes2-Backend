using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Application.Common.Interfaces.JWT;
using Domain.DTOs;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _db;
        private readonly IJWTService jWTService;
        private readonly AuthDbContext authDb;
        private readonly RoleManager<AppRole> roleManager;
        private readonly UserManager<AppUser> userManager;

        public MenuService(AppDbContext _db,IJWTService jWTService,AuthDbContext authDb,RoleManager<AppRole> roleManager,UserManager<AppUser> userManager)
        {
            this._db = _db;
            this.jWTService = jWTService;
            this.authDb = authDb;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        public async Task<IEnumerable<MenuDTO>> GetMenu()
        {
            var jwtClaims = jWTService.DecodeToken();
            var userId = jwtClaims.UserId;

            var roles = await (from role in roleManager.Roles
                               join userRole in _db.UserRoles on role.Id equals userRole.RoleId
                               where userRole.UserId == userId
                               select role.Id).ToListAsync();

            var roleMenus = await _db.RoleMenus.Where(roleMenu => roles.Contains(roleMenu.RoleId)).GroupBy(roleMenu => roleMenu.MenuId).Select(group => new
                {
                    MenuId = group.Key,
                    EditAccess = group.Any(rm => rm.EditAccess == 1),
                    CreateAccess = group.Any(rm => rm.CreateAccess == 1),
                    DeleteAccess = group.Any(rm => rm.DeleteAccess == 1),
                    PrintAccess = group.Any(rm => rm.PrintAccess == 1)
                }).ToListAsync();


            var allMenus = await _db.AppMenus.Where(menu => roleMenus.Select(rm => rm.MenuId).Contains(menu.Id) && menu.IsDeleted == 0).AsNoTracking().ToListAsync();

            var menuLookup = allMenus.ToLookup(m => m.ParentId);
            var rootMenus = menuLookup[null];

            return BuildMenuHierarchy(rootMenus, menuLookup, null, roleMenus);
        }
      
        private IEnumerable<MenuDTO> BuildMenuHierarchy(
            IEnumerable<AppMenu> rootMenus,
            ILookup<int?, AppMenu> menuLookup,
            string parentMenuCode,
            IEnumerable<dynamic> roleMenus)
        {
            var sortedMenus = rootMenus.OrderBy(m => m.Order);
            foreach (var menu in sortedMenus)
            {
                var currentMenuCode = string.IsNullOrEmpty(parentMenuCode) ? menu.Title.ToLower() : $"{parentMenuCode}.{menu.Title.ToLower()}";

                // Find the role menu entry for this menu
                var roleMenu = roleMenus.FirstOrDefault(rm => rm.MenuId == menu.Id);

                var menuDto = new MenuDTO
                {
                    MenuId = menu.Id,
                    Id = currentMenuCode,
                    Title = menu.Title,
                    Subtitle = menu.Subtitle,
                    Type = menu.Type.ToString(),
                    Icon = menu.Icon,
                    Link = menu.Link,
                    IsMenuItem = menu.IsMenuItem,
                    ParentId = menu.ParentId,
                    EditAccess = roleMenu?.EditAccess ?? false,
                    CreateAccess = roleMenu?.CreateAccess ?? false,
                    DeleteAccess = roleMenu?.DeleteAccess ?? false,
                    PrintAccess = roleMenu?.PrintAccess ?? false,
                    Children = BuildMenuHierarchy(menuLookup[menu.Id], menuLookup, currentMenuCode, roleMenus).ToList()
                };
                yield return menuDto;
            }
        }

    }
}
