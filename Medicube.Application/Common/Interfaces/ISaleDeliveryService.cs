using Domain.DTOs;

namespace Application.Common.Interfaces
{
    public interface ISaleDeliveryService
    {
        Task<object> AddSaleDelivery(SaleDeliveryDTO dto);
        Task<object> UpdateSaleDelivery(SaleDeliveryDTO dto);
        Task<object> GetIDeliveryStatus();
        Task<object> GetSaleDetailById(int deliveryId);
        Task<object> GetAllSaleDelivery(SaleDeliveryFilter req);
        Task<object> DeleteSale(int deliveryId);
        Task<object> MarkAsDelivered(int deliveryId);
        Task<object> GetArchivedList();
        Task<object> UpdateArchivedById(int deliveryId);
    }
}
