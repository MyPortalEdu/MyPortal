using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("LessonPlans")]
    public class LessonPlan : AuditableEntity, IDirectoryEntity
    {
        public Guid StudyTopicId { get; set; }

        public Guid DirectoryId { get; set; }

        public int Order { get; set; }
        
        [Required]
        [StringLength(256)]
        public required string Title { get; set; }

        [Required]
        public required string PlanContent { get; set; }

        public Directory? Directory { get; set; }
        public StudyTopic? StudyTopic { get; set; }
    }
}