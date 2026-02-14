using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("productstock")]
    public class ProductStock
    {
        [Key]
        [Column("stockid")]
        public int StockId { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("stockInOut")]
        public int StockInOut { get; set; }


        [Column("qty")]
        public decimal Quantity { get; set; }

        [Column("unitprice")]
        public decimal UnitPrice { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("referencestxt")]
        public string? References { get; set; }

        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }
    }
}
