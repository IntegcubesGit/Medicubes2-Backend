using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("ifbrscenario")]
    public class IFBRScenario
    {
        [Key]
        [Column("fbrscenariotestid")]
        public int FBRScenarioTestId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("isdeleted")]
        public int IsDeleted { get; set; } = 0;

        [Column("scenarioid")]
        public string ScenarioId { get; set; } = string.Empty;
    }
}
