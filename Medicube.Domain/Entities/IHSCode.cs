using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("ihscode")]
    public class IHSCode
    {
        [Key]
        [Column("hscodeid")]
        public int HSCodeId { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
        [Column("description")]
        public string? description { get; set; }

        public string? Description { get; set; }
    }
}
