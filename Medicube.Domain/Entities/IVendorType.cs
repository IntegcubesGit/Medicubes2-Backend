using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("ivendortype")]
    public class IVendorType
    {
        [Key]
        [Column("vendortypeid")]
        public int VendorTypeId { get; set; }

        [Column("vendortype")]
        public string? VendorType { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
