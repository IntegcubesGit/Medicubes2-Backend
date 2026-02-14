using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrgInfoController : ControllerBase
    {
        private readonly IOrgInfoService _orgInfoService;

        public OrgInfoController(IOrgInfoService orgInfoService)
        {
            _orgInfoService = orgInfoService;
        }
        [AllowAnonymous]
        [HttpPost("RegisterTenant")]
        public async Task<IActionResult> RegisterTenant([FromBody] OrgInfoDTO dto)
        {
            var res = _orgInfoService.AddTenantRegistration(dto);
            return Ok(res);
        }
    }
}
