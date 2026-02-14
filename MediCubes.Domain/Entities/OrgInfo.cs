using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("org_info")]
    public class OrgInfo
    {
        [Key]
        [Column("orgid")]
        public int OrgId { get; set; }

        [Required]
        [Column("code")]
        public string? Code { get; set; }

        [Required]
        [Column("name")]
        public string? Name { get; set; }

        [Column("statusid")]
        public int StatusId { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

    }

}
