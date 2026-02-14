using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("istate")]
    public class IStateType
    {
        [Key]
        [Column("stateid")]
        public int StateId { get; set; }

        [Column("statename")]
        public string? StateName { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
