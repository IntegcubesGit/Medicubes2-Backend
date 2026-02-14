using Microsoft.AspNetCore.Mvc;

namespace WebApi.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuAccessAttribute : TypeFilterAttribute
    {
        public MenuAccessAttribute(string menuName, string accessType) : base(typeof(MenuAccessFilter))
        {
            Arguments = new object[] { menuName, accessType };
        }

    }
}
