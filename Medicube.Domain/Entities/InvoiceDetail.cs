using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("invoicedetail")]
    public class InvoiceDetail
    {
        [Key]
        [Column("invoicedtlid")]
        public int InvoiceDtlId { get; set; }

        [Column("invoiceid")]
        public int InvoiceId { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("qty")]
        public decimal Qty { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("discount")]
        public decimal Discount { get; set; } = 0;

        [Column("saletax")]
        public decimal SaleTax { get; set; } = 0;
    }
}
