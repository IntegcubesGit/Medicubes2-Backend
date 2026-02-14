using System.Reflection;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly ICommonService _commonService;

        public CommonController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        [HttpGet("getAllEnums")]
        public ActionResult<IEnumerable<object>> GetAllEnums()
        {
            var domainAssembly = Assembly.Load("Domain");
            if (domainAssembly == null)
                return NotFound("Domain assembly not found.");

            var enums = domainAssembly.GetTypes()
                .Where(t => t.IsEnum)
                .Select(enumType => new
                {
                    EnumName = enumType.Name,
                    Values = Enum.GetValues(enumType).Cast<object>().Select(value => new
                    {
                        Id = (int)value,
                        Name = value.ToString()
                    })
                })
                .ToList();

            if (enums.Count == 0)
                return NotFound("No enums found in the domain layer.");
            return Ok(enums);
        }

        [HttpGet("getConfigSettings")]
        public async Task<IActionResult> GetConfigSettings()
        {
            var settings = await _commonService.GetConfigSettings();
            return Ok(settings);
        }

        [HttpGet("getAllLocationsForDropDown")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _commonService.GetAllLocations();
            return Ok(locations);
        }
    }
}
