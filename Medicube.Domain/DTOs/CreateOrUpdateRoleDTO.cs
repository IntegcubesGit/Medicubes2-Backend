using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class CreateOrUpdateRoleDTO
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public required List<Menu> Menus { get; set; }
    }
    public class Menu
    {
        public int MenuId { get; set; }
        public int EditAccess { get; set; }
        public int CreateAccess { get; set; }
        public int DeleteAccess { get; set; }
        public int PrintAccess { get; set; }
    }
}
