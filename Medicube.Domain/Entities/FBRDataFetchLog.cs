using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("fbrdatafetchlogs")]
    public class FBRDataFetchLog
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("triggerat")]
        [Required]
        public DateTime TriggerAt { get; set; } = DateTime.UtcNow;

        [Column("userid")]
        [Required]
        public int UserId { get; set; }

        [Column("ihscode_count")]
        public int IhsCodeCount { get; set; } = 0;

        [Column("ihscode_error")]
        public string? IhsCodeError { get; set; }

        [Column("istate_count")]
        public int IStateCount { get; set; } = 0;

        [Column("istate_error")]
        public string? IStateError { get; set; }

        [Column("iinvoicetype_count")]
        public int IInvoiceTypeCount { get; set; } = 0;

        [Column("iinvoicetype_error")]
        public string? IInvoiceTypeError { get; set; }

        [Column("isroitemcode_count")]
        public int ISroItemCodeCount { get; set; } = 0;

        [Column("isroitemcode_error")]
        public string? ISroItemCodeError { get; set; }

        [Column("isaletype_count")]
        public int ISaleTypeCount { get; set; } = 0;

        [Column("isaletype_error")]
        public string? ISaleTypeError { get; set; }

        [Column("iunittype_count")]
        public int IUnitTypeCount { get; set; } = 0;

        [Column("iunittype_error")]
        public string? IUnitTypeError { get; set; }

        [Column("itaxrate_count")]
        public int ITaxRateCount { get; set; } = 0;

        [Column("itaxrate_error")]
        public string? ITaxRateError { get; set; }

        [Column("isroschedule_count")]
        public int ISroScheduleCount { get; set; } = 0;

        [Column("isroschedule_error")]
        public string? ISroScheduleError { get; set; }
    }
}

