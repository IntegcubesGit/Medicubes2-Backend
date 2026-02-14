using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("icustregtype")]
    public class ICustRegType
    {
        [Key]
        [Column("regtypeid")]
        public int RegTypeId { get; set; }

        [Column("custregname")]
        public string? CustRegName { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
