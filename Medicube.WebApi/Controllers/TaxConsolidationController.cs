using Application.Common.Interfaces;
using Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxConsolidationController : ControllerBase
    {
        private readonly ITaxConsolidationService _taxConsolidationService;

        public TaxConsolidationController(ITaxConsolidationService taxConsolidationService)
        {
            _taxConsolidationService = taxConsolidationService;
        }

        [HttpPost("GetInvoiceDetails")]
        public async Task<IActionResult> GetInvoiceDetails(GetInvoiceDetailRequestDTO dto)
        {
            var result = await _taxConsolidationService.GetInvoiceDetails(dto);
            return Ok(result);
        }

        [HttpPost("AddChallanDetails")]
        public async Task<IActionResult> AddChallanDetails(List<ChallanRequestDTO> dtos)
        {
            var result = await _taxConsolidationService.AddChallanDetails(dtos);
            return Ok(result);
        }

        [HttpGet("GetTaxConsolidationList")]
        public async Task<IActionResult> GetTaxConsolidationList()
        {
            var result = await _taxConsolidationService.GetTaxConsolidationList();
            return Ok(result);
        }

        [HttpPost("UpdateTaxConsolidationRecord")]
        public async Task<IActionResult> UpdateTaxConsolidationRecord(UpdateTaxConsolidationRecsRequestDTO dto)
        {
            var result = await _taxConsolidationService.UpdateTaxConsolidationRecord(dto);
            return Ok(result);
        }

        [HttpGet("GetTaxConsolidationByTaxChallanId/{TaxChallanId}")]
        public async Task<IActionResult> GetTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var result = await _taxConsolidationService.GetTaxConsolidationByTaxChallanId(TaxChallanId);
            return Ok(result);
        }

        [HttpGet("GetTaxConsolidationDetailsByTaxChallanId/{TaxChallanId}")]
        public async Task<IActionResult> GetTaxConsolidationDetailsByTaxChallanId(int TaxChallanId)
        {
            var result = await _taxConsolidationService.GetTaxConsolidationDetailsByTaxChallanId(TaxChallanId);
            return Ok(result);
        }

        [HttpDelete("ArchiveTaxConsolidationByTaxChallanId/{TaxChallanId}")]
        public async Task<IActionResult> ArchiveTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var result = await _taxConsolidationService.ArchiveTaxConsolidationByTaxChallanId(TaxChallanId);
            return Ok(result);
        }

        [HttpDelete("UnArchiveTaxConsolidationByTaxChallanId/{TaxChallanId}")]
        public async Task<IActionResult> UnArchiveTaxConsolidationByTaxChallanId(int TaxChallanId)
        {
            var result = await _taxConsolidationService.UnArchiveTaxConsolidationByTaxChallanId(TaxChallanId);
            return Ok(result);
        }

        [HttpGet("GetAllArchivedTaxConsolidationList")]
        public async Task<IActionResult> GetAllArchivedTaxConsolidationList()
        {
            var result = await _taxConsolidationService.GetAllArchivedTaxConsolidationList();
            return Ok(result);
        }

    }
}
