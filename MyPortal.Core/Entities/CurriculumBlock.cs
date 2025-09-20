using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumBlocks")]
    public class CurriculumBlock : Entity
    {
        [Required]
        [StringLength(10)] 
        public required string Code { get; set; }
        
        [StringLength(256)]
        public string? Description { get; set; }
    }
}