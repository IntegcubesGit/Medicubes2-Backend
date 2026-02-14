using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("GetCustomerGrowth")]
        public async Task<IActionResult> GetCustomerGrowth()
        {
            var result = await _dashboardService.GetCustomerGrowth();
            return Ok(result);
        }
        [HttpGet("GetProductStatusDistribution")]
        public async Task<IActionResult> GetProductStatusDistribution()
        {
            var result = await _dashboardService.GetProductStatusDistribution();
            return Ok(result);
        }
        [HttpGet("GetTotalSaleAndUser")]
        public async Task<IActionResult> GetTotalSaleAndUser()
        {
            var result = await _dashboardService.GetTotalSaleAndUser();
            return Ok(result);
        }
    }
}
