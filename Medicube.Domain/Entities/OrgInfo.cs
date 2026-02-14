using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("orginfo")]
    public class OrgInfo
    {
        [Key]
        [Column("tenantid")]
        public int TenantId { get; set; }

        [Required]
        [Column("tenantcode")]
        public string? TenantCode { get; set; }

        [Required]
        [Column("tenantname")]
        public string? TenantName { get; set; }

        [Column("statusid")]
        public int StatusId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

    }

}
