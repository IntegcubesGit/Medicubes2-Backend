using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace Domain.Entities
{
    [Table("appuser")]
    public class AppUser: IdentityUser<int>
    {
        [Column("userid")]
        public override int Id { get; set; }

        [Column("name")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Column("username")]
        public override string UserName { get; set; }

        [Column("email")]
        public override string Email { get; set; }

        [Column("plaintextpassword")]
        [MaxLength(200)]
        public string PlainTextPassword { get; set; } = string.Empty;

        [Column("pwdexpireon")]
        public DateTime? PwdExpireOn { get; set; }

        [Column("roleid")]
        public int? RoleId { get; set; }

        [Column("reglocid")]
        public int? RegLocId { get; set; }

        [Column("cansignrpt")]
        public bool CanSignRpt { get; set; } = false;

        [Column("empid")]
        public int? EmpId { get; set; }

        [Column("shownameonrpt")]
        public int ShowNameOnRpt { get; set; } = 0;

        [Column("createdby")]
        public int? CreatedBy { get; set; }

        [Column("createdon")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Column("modifiedby")]
        public int? ModifiedBy { get; set; }

        [Column("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [Column("isdeleted")]
        public int IsDeleted { get; set; } = 0;

        [Column("issuperadmin")]
        public int IsSuperAdmin { get; set; } = 0;

        [Column("tenantid")]
        public int TenantId { get; set; } = 0;

        [Column("orgid")]
        public int? OrgId { get; set; }

        [Column("qualification")]
        [MaxLength(150)]
        public string? Qualification { get; set; }

        [Column("staffid")]
        public int? StaffId { get; set; }

        [Column("reportnote")]
        [MaxLength(500)]
        public string? ReportNote { get; set; }

        [Column("discountlimit")]
        public int? DiscountLimit { get; set; } = 0;

        [Column("outletid")]
        public int? OutLetId { get; set; }

        [Column("userstamp")]
        [MaxLength(200)]
        public Byte[]? UserStamp { get; set; }

        [Column("mincash")]
        public int? MinCash { get; set; } = 0;

        [Column("billaccess")]
        public int? BillAccess { get; set; }

        [Column("storeid")]
        public int? StoreId { get; set; }

        [Column("shareon")]
        public int ShareOn { get; set; } = 0;

        [Column("opdcounterid")]
        public int? OpdCounterId { get; set; }

        [Column("isapiuser")]
        public int IsAPIUser { get; set; } = 0;

        [Column("bboutletid")]
        public int? BBOutletId { get; set; }
        [Column("status")]
        public int Status { get; set; }
    
    }
}
