using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("isaletype")]
    public class ISaleType
    {
        [Key]
        [Column("saletypeid")]
        public int SaleTypeId { get; set; }

        [Column("salename")]
        public string? SaleName {  get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
