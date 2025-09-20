using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumYearGroups")]
    public class CurriculumYearGroup : Entity
    {
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }

        public int KeyStage { get; set; }
        
        [Required]
        [StringLength(10)]
        public required string Code { get; set; }
    }
}