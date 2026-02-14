using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class ProductStockDTO
    {
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
        public string? References { get; set; }

    }
}
