using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("invoiceproductslog")]
    public class InvoiceProductLog
    {
        [Key]
        [Column("invoiceproductlogid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceProductLogId { get; set; }

        [Column("productid")]
        [Required]
        public int ProductId { get; set; }

        [Column("invoiceid")]
        [Required]
        public int InvoiceId { get; set; }

        [Column("rateid")]
        public int? RateId { get; set; }

        [Column("sroid")]
        public int? SroId { get; set; }

        [Column("sroitemid")]
        public int? ItemId { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}

