using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService menu;

        public MenuController(IMenuService menu)
        {
            this.menu = menu;
        }

        [HttpGet]
        [Route("GetMenu")]
        public async Task<IActionResult> GetMenu()
        {
            var menuList = await menu.GetMenu();
            return Ok(menuList);
        }
    }
}
