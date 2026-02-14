using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("StockHistoryLogs")]
    public class StockHistoryLogs
    {
        [Key]
        [Column("LogId")]
        public int LogId { get; set; }

        [Required]
        [Column("ProdId")]
        public int ProdId { get; set; }

        [Required]
        [Column("StockQty")]
        public decimal StockQty { get; set; }

        [Required]
        [Column("UnitPrice")]
        public decimal? UnitPrice { get; set; }

        [Required]
        [Column("Reference")]
        public string Reference { get; set; } = string.Empty;

        [Required]
        [Column("Notes")]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }
    }
}
