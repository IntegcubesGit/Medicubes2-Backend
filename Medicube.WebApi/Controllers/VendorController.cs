using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {

        private readonly IVendorService _VendorService;

        public VendorController(IVendorService VenderService)
        {
            _VendorService = VenderService;
        }
        [HttpGet("GetAllVendor/{vendorTypeId}/{StateTypeId}")]
        public async Task<IActionResult> GetAllVendor(int vendorTypeId, int StateTypeId)
        {
            var result = await _VendorService.GetAllVendor(vendorTypeId, StateTypeId);
            return Ok(result);
        }
        [HttpGet("GetVendorType")]
        public async Task<IActionResult> GetVendorType()
        {
            var result = await _VendorService.GetVendorType();
            return Ok(result);
        }
        [HttpGet("GetStateType")]
        public async Task<IActionResult> GetStateType()
        {
            var result = await _VendorService.GetStateType();
            return Ok(result);
        }
        [HttpGet("GetVendorById/{VendorId}")]
        public async Task<IActionResult> GetVendorById(int VendorId)
        {
            var result = await _VendorService.GetVendorById(VendorId);
            return Ok(result);
        }
        [HttpPost("AddVendor")]
        public async Task<IActionResult> AddVendor([FromBody] VendorDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _VendorService.AddVendor(dto);
            return Ok(result);
        }
        [HttpPost("UpdateVendor")]
        public async Task<IActionResult> UpdateVendor([FromBody] VendorDTO dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "Invalid data" });

            var result = await _VendorService.UpdateVendor(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteVendor/{VendorId}")]
        public async Task<IActionResult> DeleteVendor(int VendorId)
        {

            var result = await _VendorService.DeleteVendor(VendorId);
            return Ok(result);
        }
        [HttpGet("UpdateArchivedById/{VendorId}")]
        public async Task<IActionResult> UpdateArchivedById(int VendorId)
        {
            var result = await _VendorService.UpdateArchivedById(VendorId);
            return Ok(result);
        }

        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _VendorService.GetArchivedList();
            return Ok(result);
        }
    }
}
