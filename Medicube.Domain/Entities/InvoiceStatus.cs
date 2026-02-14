using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("invoicestatus")]
    public class InvoiceStatus
    {
        [Column("invoicestatusid")]
        public int InvoiceStatusId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("isdeleted")]
        public int IsDeleted { get; set; } = 0;
    }
}
