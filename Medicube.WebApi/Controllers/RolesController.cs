using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using WebApi.ActionFilters;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService rolesRepository;

        public RolesController(IRoleService roles)
        {
            this.rolesRepository = roles;
        }

        [HttpGet]
        [Route("GetRolesList")]
        public async Task<IActionResult> GetRolesList()
        {
            var response = await rolesRepository.GetRolesList();
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateRole")]
        [MenuAccess("Roles", "createaccess")]

        public async Task<IActionResult> CreateRole([FromBody] CreateOrUpdateRoleDTO roleWithMenus)
        {
            var createdRole = await rolesRepository.CreateRole(roleWithMenus);
            return Ok(createdRole);
        }

        [HttpPut]
        [Route("UpdateRole")]
        public async Task<IActionResult> UpdateRole([FromBody] CreateOrUpdateRoleDTO roleWithMenus)
        {
            var updatedRole = await rolesRepository.UpdateRole(roleWithMenus);
            return Ok(updatedRole);
        }


        [HttpGet("getAllMenusListForCreatingRoles")]
        public async Task<IActionResult> GetAllMenusListForCreatingRoles()
        {
            var menuList = await rolesRepository.GetAllMenusListForCreatingRoles();
            return Ok(menuList);
        }

        [HttpGet("GetRoleById/{RoleId}")]
        public async Task<IActionResult> GetRoleById([FromRoute] int RoleId)
        {
            var role = await rolesRepository.GetRoleById(RoleId);
            return Ok(role);
        }

        [HttpDelete("DeleteRoleById/{RoleId}")]
        public async Task<IActionResult> DeleteRoleById([FromRoute] int RoleId)
        {
            var role = await rolesRepository.DeleteRoleById(RoleId);
            return Ok(role);
        }
    }
}
