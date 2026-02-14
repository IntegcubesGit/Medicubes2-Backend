using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class FBRDataFetchController : ControllerBase
    {
        private readonly IFBRDataFetchService _fbrDataFetchService;

        public FBRDataFetchController(IFBRDataFetchService fbrDataFetchService)
        {
            _fbrDataFetchService = fbrDataFetchService;
        }

        [HttpGet("TriggerFBRDataFetch")]
        public async Task<IActionResult> TriggerFBRDataFetch()
        {
            try
            {
                var result = await _fbrDataFetchService.FetchAllFBRData();
                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner Exception: {ex.InnerException.Message}";
                }
                return StatusCode(500, new { message = "Error triggering FBR data fetch", error = errorMessage, stackTrace = ex.StackTrace });
            }
        }
    }
}

