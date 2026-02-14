using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("invstorestock")]
    public class InvStoreStock
    {
        [Key]
        [Column("stockid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StockId { get; set; }

        [Column("transdate")]
        public DateTime TransDate { get; set; }

        [Column("storeid")]
        public int StoreId { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("quantity")]
        public decimal? Quantity { get; set; }

        [Column("itemtransid")]
        public short ItemTransId { get; set; }

        [Column("extid")]
        public int ExtId { get; set; }

        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("orgid")]
        public int OrgId { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("invid")]
        public int InvId { get; set; }

        [Column("purchseprice")]
        public decimal? PurchsePrice { get; set; }

        [Column("locid")]
        public int LocId { get; set; }
        [Column("reference")]
        public string? Reference { get; set; }
        [Column("notes")]
        public string? Notes { get; set; }
    }
}
