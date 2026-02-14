using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class RegisterUserDTO
    {
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public int IsSuperAdmin { get; set; } = 0;
        public int RoleId { get; set; } = 1;
        public int RegLocId { get; set; } = 1;
    }
}
