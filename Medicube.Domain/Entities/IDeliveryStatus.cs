using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("ideliverystatus")]
    public class IDeliveryStatus
    {
        [Key]
        [Column("deliverystatusid")]
        public int DeliveryStatusId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
