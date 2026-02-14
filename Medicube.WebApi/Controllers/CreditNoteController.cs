using Application.Common.Interfaces;
using Domain.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditNoteController : ControllerBase
    {
        private readonly ICreditNoteService _creditNoteService;

        public CreditNoteController(ICreditNoteService creditNoteService)
        {
            _creditNoteService = creditNoteService;
        }

        [HttpPost("AddCreditNotes")]
        public async Task<IActionResult> AddCreditNotes([FromBody] CreditNoteDTO dto)
        {
            var result = await _creditNoteService.AddCreditNote(dto);
            return Ok(result);
        }

        [HttpDelete("DeleteCreditNotes/{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(int invoiceId)
        {
            var result = await _creditNoteService.DeleteCreditNotes(invoiceId);
            return Ok(result);
        }

        [HttpGet("GetCreditNotesByInvoiceId/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceDetailById(int invoiceId)
        {
            var result = await _creditNoteService.GetInvoiceDetailById(invoiceId);
            return Ok(result);
        }

        [HttpPost("GetAllCreditNoteList")]
        public async Task<IActionResult> GetAllCreditNoteList(CreditNoteListDTO req)
        {
            var result = await _creditNoteService.GetAllCreditNoteList(req);
            return Ok(result);
        }
        [HttpGet("GetIReasonType")]
        public async Task<IActionResult> GetIReasonType()
        {
            var result = await _creditNoteService.GetIReasonType();
            return Ok(result);
        }
        [HttpGet("GetICreditStatus")]
        public async Task<IActionResult> GetICreditStatus()
        {
            var result = await _creditNoteService.GetICreditStatus();
            return Ok(result);
        }
        [HttpGet("GetArchivedList")]
        public async Task<IActionResult> GetArchivedList()
        {
            var result = await _creditNoteService.GetArchivedList();
            return Ok(result);
        }
        [HttpGet("UpdateArchivedById/{invoiceId}")]
        public async Task<IActionResult> UpdateArchivedById(int invoiceId)
        {
            var result = await _creditNoteService.UpdateArchivedById(invoiceId);
            return Ok(result);
        }

    }
}
