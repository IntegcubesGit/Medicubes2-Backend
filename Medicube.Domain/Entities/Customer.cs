using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("customer")]
    public class Customer
    {
        [Key]
        public int customerid { get; set; }
        public string? name { get; set; }
        public int customertypeid { get; set; }
        public int custregtypeid { get; set; }
        public string? regno { get; set; }
        public string? address { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public int stateprovince { get; set; }
        public short isdeleted { get; set; } = 0;
        public int createdby { get; set; }
        public DateTime createdon { get; set; } = DateTime.UtcNow;
        public int? modifiedby { get; set; }
        public DateTime? modifiedon { get; set; }
        public int? tenantid { get; set; }
        public string? cnic { get; set; }
    }
}
