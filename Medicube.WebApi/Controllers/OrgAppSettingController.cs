using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgAppSettingController : ControllerBase
    {
        private readonly IOrgAppSettingService _settingService;

        public OrgAppSettingController(IOrgAppSettingService settingService)
        {
            _settingService = settingService;
        }
        [HttpPost("AddorUpdateSetting")]
        public async Task<IActionResult> AddorUpdateSetting(OrgSettingDTO settingDTO)
        {
            var res = await _settingService.AddOrUpdate(settingDTO);
            return Ok(res);
        }
        [HttpGet("GetICurrency")]
        public async Task<IActionResult> GetICurrency()
        {
            var res = await _settingService.GetAllCurrency();
            return Ok(res);
        }
        [HttpGet("GetOrgAppSetting")]
        public async Task<IActionResult> GetOrgAppSetting()
        {
            var res = await _settingService.GetOrgSetting();
            return Ok(res);
        }
    }
}
