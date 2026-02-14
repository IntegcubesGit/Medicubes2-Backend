using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AppRoleAppMenu
    {
        public int AppRoleAppMenuId { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public int EditAccess { get; set; }
        public int CreateAccess { get; set; }
        public int DeleteAccess { get; set; }
        public int PrintAccess { get; set; }

    }
}
