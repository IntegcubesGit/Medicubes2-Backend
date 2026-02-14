using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("configsetting")]
    public class ConfigSetting
    {
        [Key]
        public int id { get; set; }
        public int tenantid { get; set; }
        public int clientsettingid { get; set; } = 0;
        public int isdeleted { get; set; } = 0;
    }
}
