using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    [Table("app_menu")]
    public class AppMenu
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public MenuItemTypeEnum Type { get; set; }
        public string? Icon { get; set; }
        public string? Link { get; set; }
        public int? ParentId { get; set; }
        public int IsDeleted { get; set; }
        public int Order { get; set; }
        public virtual ICollection<AppMenu> Children { get; set; } = new List<AppMenu>();
        public virtual AppMenu? Parent { get; set; }
        [Column("orgid")]
        public int OrgId { get; set; }
        public int IsMenuItem { get; set; }
    }
}
