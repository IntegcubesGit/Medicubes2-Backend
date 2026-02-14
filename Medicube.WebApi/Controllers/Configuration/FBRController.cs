using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Configuration
{
    [Route("api/[controller]")]
    [ApiController]
    public class FBRController : ControllerBase
    {
        private readonly IFBRService _fbrService;

        public FBRController(IFBRService iFBRService)
        {
            _fbrService = iFBRService;
        }

        [HttpPost("SubmitInvoiceToFBR")]
        public async Task<IActionResult> SubmitInvoiceToFBR([FromBody] FBRInvoiceSubmissionDTO dto)
        {
            var result = await _fbrService.SubmitInvoiceToFBR(dto);
            return Ok(result);
        }
     }
}
