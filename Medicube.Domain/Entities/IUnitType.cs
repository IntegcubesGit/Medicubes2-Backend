using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("iunittype")]
    public class IUnitType
    {
        [Key]
        [Column("unitid")]
        public int UnitId { get; set; }

        [Column("unitname")]
        public string? UnitName { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
