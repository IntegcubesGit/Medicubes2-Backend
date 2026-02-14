using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("saledeliverydetail")]
    public class SaleDeliveryDetail
    {
        [Key]
        [Column("saledtlid")]
        public int SaleDtlId { get; set; }
        [Column("saledeliveryid")]
        public int SaleDeliveryId { get; set; }

        [Column("itemid")]
        public int ItemId { get; set; }

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("unitprice")]
        public decimal UnitPrice { get; set; }

        [Column("totalprice")]
        public decimal TotalPrice { get; set; }
    }
}
