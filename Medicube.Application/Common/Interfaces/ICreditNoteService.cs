using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface ICreditNoteService
    {
        Task<object> AddCreditNote(CreditNoteDTO dto);
        Task<object> GetAllCreditNoteList(CreditNoteListDTO req);
        Task<object> GetInvoiceDetailById(int invoiceId);
        Task<object> DeleteCreditNotes(int invoiceId);
        Task<object> GetArchivedList();
        Task<object> GetIReasonType();
        Task<object> GetICreditStatus();
        Task<object> UpdateArchivedById(int invoiceId);
    }
}
