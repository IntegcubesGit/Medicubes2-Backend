using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("app_config")]
    public class ConfigSetting
    {
        [Key]
        public int id { get; set; }
        [Column("orgid")]
        public int OrgId { get; set; }
        public int clientsettingid { get; set; } = 0;
        public int isdeleted { get; set; } = 0;
    }
}
