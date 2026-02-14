using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> Register([FromBody] RegisterOrUpdateUserRequestDTO registerUserRequest)
        {
            var roles = registerUserRequest.Roles;

            var registerUserResponse = await _userService.RegisterUser(registerUserRequest, roles);

            return Ok(registerUserResponse);
        }
        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] RegisterOrUpdateUserRequestDTO updateUserRequest)
        {
            var roles = updateUserRequest.Roles;

            var updatedUserResponse = await _userService.UpdateUser(updateUserRequest, roles);

            return Ok(updatedUserResponse);
        }
        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page, [FromQuery] int size, [FromQuery] string sort, [FromQuery] string order, [FromQuery] string? search)
        {
            var users = await _userService.GetAllUsers(page, size, sort, order, search);
            return Ok(users);
        }
        [HttpGet("getRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _userService.GetRoles();
            return Ok(roles);
        }
        [HttpGet("getUserById")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetUserById(userId);
            return Ok(user);
        }
        [HttpGet("getUserInfoById/{userId}")]
        public async Task<IActionResult> GetUserInfoById([FromRoute] int userId)
        {
            var user = await _userService.GetUserInfoById(userId);
            return Ok(user);
        }
        [HttpGet("GetOrgLocationsById/{userId}")]
        public async Task<IActionResult> GetOrgLocationsById(int userId)
        {
            var user = await _userService.GetOrgLocationsById(userId);
            return Ok(user);
        }
        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var deletedUserResponse = await _userService.DeleteUser(userId);
            return Ok(deletedUserResponse);
        }
        [HttpGet]
        [Route("GetUserInformationByParsingJWT")]
        public async Task<IActionResult> GetUserInformationByParsingJWT()
        {
            var user = await _userService.GetUserInformationByParsingJWT();
            if (user is StatusCodeResult statusCodeResult && statusCodeResult.StatusCode == 401)
            {
                return Unauthorized();
            }
            return Ok(user);
        }
    }
}
