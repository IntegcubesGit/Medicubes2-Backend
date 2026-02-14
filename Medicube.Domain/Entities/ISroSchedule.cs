using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("isroschedule")]
    public class ISroSchedule
    {
        [Key]
        [Column("sroid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SroId { get; set; }

        [Column("serno")]
        [Required]
        public int SerNo { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("rateid")]
        [Required]
        public int RateId { get; set; }

        [Column("stateid")]
        [Required]
        public int StateId { get; set; }

        [Column("isdeleted")]
        public short IsDeleted { get; set; } = 0;
    }
}

