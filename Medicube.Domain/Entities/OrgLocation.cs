using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("OrgLocation")]
    public class OrgLocation
    {
        [Key]
        public int LocId { get; set; }
        public int OrgId { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? Address { get; set; }
        public string? TimingHead { get; set; }
        public string? ComplaintCell { get; set; }
        public short StatusId { get; set; }
        public int IsMain { get; set; }
        public int GrpId { get; set; }
        public int DispatchTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string? CutOffTime { get; set; }
        public long? POSID { get; set; }
        public int IsActive { get; set; }
        public int UnSpecified { get; set; }
        public int BRTypeId { get; set; }
        public int DefaultInstId { get; set; }
        public int PriceId { get; set; }
        public string? LogoPath { get; set; }
        public int ShowOnApp { get; set; }
        public int TenantId { get; set; }
    }
}
