using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class OrgInfoDTO
    {
        public string? TenantCode { get; set; }
        public string? TenantName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public int CanSignRpt { get; set; }
        public int ShowNameOnRpt { get; set; }
        public int RelevantStaffId { get; set; }
        public int DiscountLimit { get; set; }
        public int MinCash { get; set; }
        public int EmpNo { get; set; }
        public string? Qualification { get; set; }
        public string ReportNote { get; set; }
        public string? DoctorStamp { get; set; }
        public int RegLocId { get; set; }
    }
}
