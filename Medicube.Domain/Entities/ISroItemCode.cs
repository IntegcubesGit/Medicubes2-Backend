using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("isroitemcode")]
    public class ISroItemCode
    {
        [Key]
        [Column("schitemid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SchItemId { get; set; }

        [Column("sroitemid")]
        public int SROItemId { get; set; }

        [Column("sroitemtitle")]
        public string? SroItemTitle { get; set; }

        [Column("SroId")]
        [Required]
        public int SroId { get; set; }

        [Column("isDeleted")]
        public int IsDeleted { get; set; } = 0;
    }
}

