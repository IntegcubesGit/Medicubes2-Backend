using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleDeliveryController : ControllerBase
    {
        private readonly ISaleDeliveryService _deliveryService;

        public SaleDeliveryController(ISaleDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [HttpGet("GetDeliveryStatus")]
        public async Task<IActionResult> GetDeliveryStatus()
        {
            var result = await _deliveryService.GetIDeliveryStatus();
            return Ok(result);
        }

        [HttpGet("GetSaleDeliveryById/{deliveryId}")]
        public async Task<IActionResult> GetSaleDeliveryById(int deliveryId)
        {
            var result = await _deliveryService.GetSaleDetailById(deliveryId);
            return Ok(result);
        }

        [HttpPost("GetAllSaleDelivery")]
        public async Task<IActionResult> GetAllSaleDelivery(SaleDeliveryFilter req)
        {
            var result = await _deliveryService.GetAllSaleDelivery(req);
            return Ok(result);
        }

        [HttpGet("MarkAsDelivered/{deliveryId}")]
        public async Task<IActionResult> MarkAsDelivered(int deliveryId)
        {
            var result = await _deliveryService.MarkAsDelivered(deliveryId);
            return Ok(result);
        }
        [HttpPost("AddSaleDelivery")]
        public async Task<IActionResult> AddSaleDelivery([FromBody] SaleDeliveryDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request" });

            var result = await _deliveryService.AddSaleDelivery(dto);
            return Ok(result);
        }


        [HttpPut("UpdateSaleDelivery")]
        public async Task<IActionResult> UpdateSaleDelivery([FromBody] SaleDeliveryDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid request" });

            var result = await _deliveryService.UpdateSaleDelivery(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteSaleDelivery/{deliveryId}")]
        public async Task<IActionResult> DeleteSaleDelivery(int deliveryId)
        {
            var result = await _deliveryService.DeleteSale(deliveryId);
            return Ok(result);
        }

        [HttpGet("UpdateArchivedById/{deliveryId}")]
        public async Task<IActionResult> UpdateArchivedById(int deliveryId)
        {
            var result = await _deliveryService.UpdateArchivedById(deliveryId);
            return Ok(result);
        }

        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _deliveryService.GetArchivedList();
            return Ok(result);
        }

    }
}
