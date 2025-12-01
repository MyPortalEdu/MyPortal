using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LessonPlanTemplates")]
    public class LessonPlanTemplate : Entity
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = null!;

        [Required]
        public string PlanTemplate { get; set; } = null!;
    }
}