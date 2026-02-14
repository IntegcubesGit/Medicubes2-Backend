using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("app_userlocation")]
    public class AppUserLocation
    {
        [Column("id")]
        public int id { get; set; }

        [Column("userid")]
        public int userid { get; set; }

        [Column("locid")]
        public int locid { get; set; }
    }
}
