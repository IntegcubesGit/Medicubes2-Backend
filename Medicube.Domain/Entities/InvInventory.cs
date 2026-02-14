using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("invinventory")]
    public class InvInventory
    {
        [Key]
        [Column("invid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvId { get; set; }

        [Column("storeid")]
        public int StoreId { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("consumedquantity")]
        public decimal ConsumedQuantity { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("itemtransid")]
        public short? ItemTransId { get; set; }

        [Column("extid")]
        public int? ExtId { get; set; }

        [Column("transdate")]
        public DateTime? TransDate { get; set; }
        [Column("tenantid")]
        public int? TenantId { get; set; }
    }
}
