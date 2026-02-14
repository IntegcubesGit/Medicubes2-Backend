using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("icustomertype")]
    public class ICustomerType
    {
        [Key]
        [Column("custtypeid")]
        public int CustTypeId { get; set; }

        [Column("custtype")]
        public string? CustType { get; set; }
        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
