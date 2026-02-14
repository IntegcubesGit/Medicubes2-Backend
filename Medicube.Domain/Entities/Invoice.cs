using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("invoice")]
    public class Invoice
    {
        [Key]
        [Column("invoiceid")]
        public int InvoiceId { get; set; }

        [Column("deliveryid")]
        public int? DeliveryId { get; set; }

        [Column("custid")]
        public int CustId { get; set; }

        [Column("originationid")]
        public int OriginationId { get; set; }

        [Column("destinationid")]
        public int DestinationId { get; set; }

        [Column("invoicenumber")]
        public string? InvoiceNumber { get; set; }
        [Column("totaldiscount")]
        public decimal TotalDiscount { get; set; }
        [Column("totaltax")]
        public decimal TotalTax { get; set; }

        [Column("invoicedate")]
        public DateTime InvoiceDate { get; set; }

        [Column("fbrtestscenarioid")]
        public int? FBRTestScenarioId { get; set; }

        [Column("paymentterm")]
        public string? PaymentTerm { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("isdeleted")]
        public int IsDeleted { get; set; }

        [Column("tenantid")]
        public int TenantId { get; set; }

        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
        [Column("statusid")]
        public int StatusId { get; set; }
        [Column("reasontypeid")]
        public int? ReasonTypeId { get; set; }
        [Column("extid")]
        public int ExtId { get; set; }
        [Column("fbrinvoicenumber")]
        public string? FBRInvoiceNumber { get; set; }
        [Column("fbrresponse")]
        public string? FBRResponse { get; set; }
        [Column("idoctypeid")]
        public int? IDocTypeId { get; set; }
        [Column("saletypeid")]
        public int? SaleTypeId { get; set; }
    }
}
