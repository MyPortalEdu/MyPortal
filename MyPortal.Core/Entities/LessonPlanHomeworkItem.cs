using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("LessonPlanHomeworkItems")]
    public class LessonPlanHomeworkItem : Entity
    {
        public Guid LessonPlanId { get; set; }

        public Guid HomeworkItemId { get; set; }

        public LessonPlan? LessonPlan { get; set; }
        public HomeworkItem? HomeworkItem { get; set; }
    }
}