using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class FBRInvoiceItemDTO
    {    
        [Required]
        public string HsCode { get; set; }

        [Required]
        public string ProductDescription { get; set; }

        public string Rate { get; set; }
        public string UoM { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalValues { get; set; }
        public decimal ValueSalesExcludingST { get; set; }
        public decimal FixedNotifiedValueOrRetailPrice { get; set; }
        public decimal SalesTaxApplicable { get; set; }
        public decimal SalesTaxWithheldAtSource { get; set; }
        public string ExtraTax { get; set; }
        public decimal FurtherTax { get; set; }
        public string SroScheduleNo { get; set; }
        public decimal FedPayable { get; set; }
        public decimal Discount { get; set; }
        public string SaleType { get; set; }
        public string SroItemSerialNo { get; set; }
    }
}
