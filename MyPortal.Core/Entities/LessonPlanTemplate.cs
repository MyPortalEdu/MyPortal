using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LessonPlanTemplates")]
    public class LessonPlanTemplate : Entity
    {
        [Required]
        [StringLength(256)]
        public required string Name { get; set; }

        [Required] 
        public required string PlanTemplate { get; set; }
    }
}