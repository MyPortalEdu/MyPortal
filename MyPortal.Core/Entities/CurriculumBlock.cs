using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumBlocks")]
    public class CurriculumBlock : Entity
    {
        [Required]
        [StringLength(10)]
        public string Code { get; set; } = null!;
        
        [StringLength(256)]
        public string? Description { get; set; }
    }
}