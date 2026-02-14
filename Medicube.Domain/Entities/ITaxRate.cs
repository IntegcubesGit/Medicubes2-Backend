using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("itaxrate")]
    public class ITaxRate
    {
        [Key]
        [Column("rateid")]
        public int RateId { get; set; }

        [Column("taxrateid")]
        [Required]
        public int TaxRateId { get; set; }

        [Column("ratetitle")]
        [Required]
        [MaxLength(255)]
        public string RateTitle { get; set; } = string.Empty;

        [Column("ratevalue", TypeName = "numeric(10,2)")]
        [Required]
        public decimal RateValue { get; set; }

        [Column("saletypeid")]
        [Required]
        public int SaleTypeId { get; set; }

        [Column("stateid")]
        [Required]
        public int StateId { get; set; }

        [Column("isdeleted")]
        public short IsDeleted { get; set; } = 0;
    }
}

