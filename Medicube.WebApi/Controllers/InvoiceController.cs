using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost("AddInvoice")]
        public async Task<IActionResult> AddInvoice([FromBody] InvoiceDTO dto)
        {
            var result = await _invoiceService.AddInvoice(dto);
            return Ok(result);
        }

        [HttpPut("UpdateInvoice")]
        public async Task<IActionResult> UpdateInvoice([FromBody] InvoiceDTO dto)
        {
            var result = await _invoiceService.UpdateInvoice(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteInvoice/{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(int invoiceId)
        {
            var result = await _invoiceService.DeleteInvoice(invoiceId);
            return Ok(result);
        }

        [HttpGet("GetInvoiceById/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceById(int invoiceId)
        {
            var result = await _invoiceService.GetInvoice(invoiceId);
            return Ok(result);
        }

        [HttpGet("GetAllInvoiceList/{status}/{custId}/{startDate}/{toDate}")]
        public async Task<IActionResult> GetAllInvoiceList(int status, int custId ,string startDate, string toDate)
        {
            var result = await _invoiceService.GetAllInvoiceList(status, custId, startDate, toDate);
            return Ok(result);
        }

        [HttpGet("GetIFBRScenarioStatus")]
        public async Task<IActionResult> GetIFBRScenarioStatus()
        {
            var result = await _invoiceService.GetIFBRScenarioStatus();
            return Ok(result);
        }
        [HttpGet("GetProductDetail")]
        public async Task<IActionResult> GetProductDetail()
        {
            var res = await _invoiceService.GetProductDetail();
            return Ok(res);
        }
        [HttpGet("UpdateArchivedById/{invoiceId}")]
        public async Task<IActionResult> UpdateArchivedById(int invoiceId)
        {
            var result = await _invoiceService.UpdateArchivedById(invoiceId);
            return Ok(result);
        }

        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _invoiceService.GetArchivedList();
            return Ok(result);
        }

        [HttpGet("GetInvoiceSlipDetail/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceSlipDetail(int invoiceId)
        {
            var result = await _invoiceService.GetInvoiceSlipDetail(invoiceId);
            return Ok(result);
        }
        [HttpGet("GetDeliveryFromInvoice/{deliveryId}")]
        public async Task<IActionResult> GetDeliveryFromInvoice(int deliveryId)
        {
            var result = await _invoiceService.GetDeliveryFromInvoice(deliveryId);
            return Ok(result);
        }
        [HttpPost("SaveSubmitFBR")]
        public async Task<IActionResult> SaveSubmitFBR(FBRResponseDTO dto)
        {
            var result = await _invoiceService.SaveSubmitFBR(dto);
            return Ok(result);
        }
          
    }
}
