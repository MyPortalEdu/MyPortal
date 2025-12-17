using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumYearGroups")]
    public class CurriculumYearGroup : Entity
    {
        [Required] 
        [StringLength(128)] 
        public string Name { get; set; } = null!;

        public int KeyStage { get; set; }

        [Required] 
        [StringLength(10)] 
        public string Code { get; set; } = null!;
    }
}