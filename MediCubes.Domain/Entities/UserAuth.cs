using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("userauth")]
    public class UserAuth
    {
        [Key]
        public int id { get; set; }
        public string token { get; set; } = string.Empty;
        public int userid { get; set; }
        public bool isrevoked { get; set; } = false;
        public DateTime expires { get; set; }
        public DateTime created { get; set; } = DateTime.UtcNow;
    }
}
