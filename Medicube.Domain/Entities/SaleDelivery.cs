using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("saledelivery", Schema = "public")]
    public class SaleDelivery
    {
        [Key]
        [Column("deliveryid")]
        public int DeliveryId { get; set; }

        [Column("deliverynumber")]
        public string? DeliveryNumber { get; set; }

        [Column("custid")]
        public int CustId { get; set; }

        [Column("deliverydate")]
        public DateTime DeliveryDate { get; set; }

        [Column("expecteddeliverydate")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Column("deliverystatusid")]
        public int DeliveryStatusId { get; set; }

        [Column("shippingaddress")]
        public string? ShippingAddress { get; set; }

        [Column("shippingmethod")]
        public string? ShippingMethod { get; set; }

        [Column("trackingnumber")]
        public string? TrackingNumber { get; set; }

        [Column("contactperson")]
        public string? ContactPerson { get; set; }

        [Column("contactphone")]
        public string? ContactPhone { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("tenantid")]
        public int TenantId { get; set; }

        [Column("createdby")]
        public int CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; }

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [Column("isdeleted")]
        public int IsDeleted { get; set; }
    }
}
