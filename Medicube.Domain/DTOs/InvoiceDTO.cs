using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class InvoiceDTO
    {
        public int InvoiceId { get; set; }
        public int? DeliveryId { get; set; }
        public int CustId { get; set; }
        public int OriginationId { get; set; }
        public int DestinationId { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int? FBRTestScenarioId { get; set; }
        public string? PaymentTerm { get; set; }
        public string? Notes { get; set; }
        public int? InvoiceTypeId { get; set; }
        public int? SaleTypeId { get; set; }
        public List<InvoiceDetailDTO>? Details { get; set; }
    }
}
