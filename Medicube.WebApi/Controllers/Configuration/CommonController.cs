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
        private readonly ICommonService commonRepository;

        public CommonController(ICommonService commonRepository)
        {
            this.commonRepository = commonRepository;
        }
        [HttpGet("getAllEnums")]
        public ActionResult<IEnumerable<string>> GetAllEnums()
        {
            var domainAssembly = Assembly.Load("Domain");

            if (domainAssembly == null)
            {
                return NotFound("Domain assembly not found.");
            }

            var enums = domainAssembly.GetTypes().Where(t => t.IsEnum).Select(enumType => new
                                      {
                                          EnumName = enumType.Name,
                                          Values = Enum.GetValues(enumType).Cast<object>().Select(value => new
                                                       {
                                                           Id = (int)value,
                                                           Name = value.ToString()
                                                       })
                                      }).ToList();

            if (enums.Count == 0)
            {
                return NotFound("No enums found in the domain layer.");
            }
            return Ok(enums);

        }

        [HttpGet("getConfigSettings")]
        public async Task<IActionResult> GetConfigSettings()
        {
            var settings = await commonRepository.GetConfigSettings();
            return Ok(settings);
        }
        [HttpGet("GetSaleDeliveryList")]
        public async Task<IActionResult> GetSaleDeliveryList()
        {
            var settings = await commonRepository.GetSaleDeliveryList();
            return Ok(settings);
        }
        [HttpGet("getAllLocationsForDropDown")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await commonRepository.GetAllLocations();
            return Ok(locations);
        }
        [HttpGet("GetSaleDeliveryInInvoiceList")]
        public async Task<IActionResult> GetSaleDeliveryInInvoiceList()
        {
            var locations = await commonRepository.GetSaleDeliveryInInvoiceList();
            return Ok(locations);
        }

        [HttpGet("GetTaxRate/{stateId}/{saleTypeId}")]
        public async Task<IActionResult> GetTaxRate(int stateId, int saleTypeId)
        {
            var taxRates = await commonRepository.GetTaxRate(stateId, saleTypeId);
            return Ok(taxRates);
        }

        [HttpGet("GetSROSchedule/{stateId}/{rateId}")]
        public async Task<IActionResult> GetSROSchedule(int stateId, int rateId)
        {
            // rateId is the primary key (RateId) from itaxrate table
            var sroSchedules = await commonRepository.GetSROSchedule(stateId, rateId);
            return Ok(sroSchedules);
        }

        [HttpGet("GetSROItemCodes/{sroId}")]
        public async Task<IActionResult> GetSROItemCodes(int sroId)
        {
            var sroItemCodes = await commonRepository.GetSROItemCodes(sroId);
            return Ok(sroItemCodes);
        }

        [HttpGet("GetAllInvoiceTypes")]
        public async Task<IActionResult> GetAllInvoiceTypes()
        {
            var invoiceTypes = await commonRepository.GetAllInvoiceTypes();
            return Ok(invoiceTypes);
        }
    }
}
