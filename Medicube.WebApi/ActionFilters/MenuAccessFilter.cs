using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace WebApi.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class MenuAccessFilter : Attribute, IAsyncActionFilter
    {
        private readonly string _menuName;
        private readonly string _accessType;
        private readonly ICurrentUser _currentUser;

        public MenuAccessFilter(string menuName, string accessType, ICurrentUser currentUser)
        {
            _menuName = menuName;
            _accessType = accessType;
            _currentUser = currentUser;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appDbContext = context.HttpContext.RequestServices.GetService(typeof(AppDbContext)) as AppDbContext;
            if (appDbContext == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var tempUser = _currentUser.GetCurrentUser();

            // Build the query using query syntax
            var query = await (from uar in appDbContext.UserRoles
                                join arm in appDbContext.RoleMenus on uar.RoleId equals arm.RoleId
                                join menu in appDbContext.AppMenus on arm.MenuId equals menu.Id
                                where uar.UserId == tempUser.UserId && menu.Title == _menuName
                                select arm).ToListAsync();


            // Dynamically check the access bit
            var hasAccess = false;

            if (_accessType.ToLower() == "editaccess")
            {
                hasAccess = query.Any(menuAccess => menuAccess.EditAccess == 1);
            }
            else if (_accessType.ToLower() == "createaccess")
            {
                hasAccess = query.Any(menuAccess => menuAccess.CreateAccess == 1);
            }
            else if (_accessType.ToLower() == "deleteaccess")
            {
                hasAccess = query.Any(menuAccess => menuAccess.DeleteAccess == 1);
            }
            else if (_accessType.ToLower() == "printaccess")
            {
                hasAccess = query.Any(menuAccess => menuAccess.PrintAccess == 1);
            }
            else
            {
                hasAccess = false; // Unknown access type
            }

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
                return;
            }

            // Proceed to the action
            await next();
        }
    }
}
