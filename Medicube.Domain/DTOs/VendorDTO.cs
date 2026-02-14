using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class VendorDTO
    {
        public int vendorid { get; set; }
        public string? vendorname { get; set; }
        public int vendortypeid { get; set; }
        public string? regno { get; set; }
        public string? vendoraddress { get; set; }
        public string? vendorphone { get; set; }
        public string? vendoremail { get; set; }
        public int stateprovince { get; set; }
    }
}
