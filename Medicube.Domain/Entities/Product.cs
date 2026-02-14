using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("product")]
    public class Product
    {
        [Key]
        public int itemid { get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public int? hscodeid { get; set; }
        public int? unitid { get; set; }
        public decimal? price { get; set; }
        public int? reorderlevel { get; set; }
        public int isactive { get; set; } = 0;
        public DateTime createdon { get; set; } = DateTime.UtcNow;
        public int? createdby { get; set; }
        public int? tenantid { get; set; }
        public int isdeleted { get; set; } = 0;
        public DateTime? modifiedon { get; set; }
        public int? modifiedby { get; set; }
    }

}
