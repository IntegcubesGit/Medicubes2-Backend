using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class MenuDTO
    {
        public int MenuId { get; set; }
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Type { get; set; }
        public string? Icon { get; set; }
        public string? Link { get; set; }
        public int? ParentId { get; set; }
        public bool EditAccess { get; set; }
        public bool CreateAccess { get; set; }
        public bool DeleteAccess { get; set; }
        public bool PrintAccess { get; set; }
        public int IsMenuItem { get; set; }
        public List<MenuDTO> Children { get; set; } = new List<MenuDTO>();
    }
}
