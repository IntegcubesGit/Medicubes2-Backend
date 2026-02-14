using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("AccTaxChallan")]
    public class AccTaxChallan
    {

        [Key]
        [Column("TaxChallanId")]
        public int TaxChallanId { get; set; }

        [Required]
        [Column("FromDate")]
        public DateTime FromDate { get; set; }

        [Required]
        [Column("ToDate")]
        public DateTime ToDate { get; set; }

        [Column("RefNumber")]
        public string RefNumber { get; set; }

        [Column("PSId")]
        public string PSId { get; set; }

        [Column("CPRNNo")]
        public string? CPRNNo { get; set; }

        [Column("TenantId")]
        public int TenantId { get; set; }

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [Column("ModifiedBy")]
        public int? ModifiedBy { get; set; }
        [Column("ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }
        [Column("IsDeleted")]
        public int IsDeleted { get; set; }
        [Column("PaymentDate")]
        public DateTime? PaymentDate { get; set; }
        [Column("ChequeNo")]
        public string? ChequeNo { get; set; }
        [Column("PaidRemarks")]
        public string? PaidRemarks { get; set; }
    }
}
