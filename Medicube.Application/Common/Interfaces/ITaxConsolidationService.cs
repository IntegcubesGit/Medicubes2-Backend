using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface ITaxConsolidationService
    {
        Task<object> GetInvoiceDetails(GetInvoiceDetailRequestDTO dto);
        Task<object> AddChallanDetails(List<ChallanRequestDTO> dtos);
        Task<object> GetTaxConsolidationList();
        Task<object> UpdateTaxConsolidationRecord(UpdateTaxConsolidationRecsRequestDTO dto);
        Task<object> GetTaxConsolidationByTaxChallanId(int TaxChallanId);
        Task<object> GetTaxConsolidationDetailsByTaxChallanId(int TaxChallanId);
        Task<object> ArchiveTaxConsolidationByTaxChallanId(int TaxChallanId);
        Task<object> UnArchiveTaxConsolidationByTaxChallanId(int TaxChallanId);
        Task<object> GetAllArchivedTaxConsolidationList();
    }
}
