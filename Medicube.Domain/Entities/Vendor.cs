using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("vendor")]
    public class Vendor
    {
        [Key]
        public int vendorid { get; set; }
        public string? vendorname { get; set; }
        public int vendortypeid { get; set; }
        public string? regno { get; set; }
        public string? vendoraddress { get; set; }
        public string? vendorphone { get; set; }
        public string? vendoremail { get; set; }
        public int stateprovince { get; set; }
        public DateTime createdon { get; set; } = DateTime.Now;
        public int createdby { get; set; }
        public int? modifiedby { get; set; }
        public DateTime? modifiedon { get; set; }
        public int? tenantid { get; set; }
        public int isdeleted { get; set; }

    }
}
