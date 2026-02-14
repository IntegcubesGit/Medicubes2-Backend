using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AccTaxChallanDetail")]
    public class AccTaxChallanDetail
    {
        [Key]
        [Column("TaxDetailId")]
        public int TaxDetailId { get; set; }

        [Required]
        [Column("TaxChallanId")]
        public int TaxChallanId { get; set; }

        [Required]
        [Column("InvoiceId")]
        public int InvoiceId { get; set; }

        [Column("TaxAmount")]
        public decimal TaxAmount { get; set; }

        [Column("TaxableAmount")]
        public decimal TaxableAmount { get; set; }

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [Column("ModifiedBy")]
        public int? ModifiedBy { get; set; }
        [Column("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }
    }
}
