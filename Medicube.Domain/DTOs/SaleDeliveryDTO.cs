using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class SaleDeliveryDTO
    {
        public int DeliveryId { get; set; }
        public decimal DeliveryNumber { get; set; }
        public int CustId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public int DeliveryStatusId { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? Notes { get; set; }
        public List<SaleDeliveryDetailDTO>? Details { get; set; }
    }
    public class SaleDeliveryFilter
    {
        public int statusId { get; set; }
        public int custId { get; set; }
        public string? startDate {  get; set; }
        public string? endDate { get; set; }

    }
}
