using Application.Common.Interfaces.Auth;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> authenticate([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.UserName, request.Password);
            return Ok(token);
        }
        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        //{
        //    var result = await _authService.RegisterUserAsync(dto);
        //    return Ok(result);
        //}
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            var res = await _authService.Logout();
            return Ok(res);
        }
    }
    public class LoginRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
