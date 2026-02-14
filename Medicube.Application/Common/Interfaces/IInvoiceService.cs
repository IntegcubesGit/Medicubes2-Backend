using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface IInvoiceService
    {
        Task<object> AddInvoice(InvoiceDTO dto);
        Task<object> DeleteInvoice(int invoiceId);
        Task<object> GetInvoice(int invoiceId);
        Task<object> UpdateInvoice(InvoiceDTO dto);
        Task<object> GetAllInvoiceList(int status, int custId, string startDate, string ToDate);
        Task<object> GetDeliveryFromInvoice(int deliveryId);
        Task<object> GetIFBRScenarioStatus();
        Task<object> GetProductDetail();
        Task<object> GetArchivedList();
        Task<object> UpdateArchivedById(int invoiceId);
        Task<object> GetInvoiceSlipDetail(int invoiceId);
        Task<object> SaveSubmitFBR(FBRResponseDTO dto);
    }
}
