using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("iinvoicetype")]
    public class IInvoiceType
    {
        [Key]
        [Column("invoicetypeid")]
        public int InvoiceTypeId { get; set; }

        [Column("invoicetypedescription")]
        [Required]
        [MaxLength(255)]
        public string InvoiceTypeDescription { get; set; } = string.Empty;
    }
}

