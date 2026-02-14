using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{

    public class TempUser
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int IsSuperAdmin { get; set; }
        /// <summary>Organization id for multi-tenant data isolation.</summary>
        public int OrgId { get; set; }
        /// <summary>Optional registered location id from token.</summary>
        public int? RegLocId { get; set; }
    }

}
