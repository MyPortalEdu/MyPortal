using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("LessonPlans")]
    public class LessonPlan : Entity, IAuditableEntity, IDirectoryEntity, IVersionedEntity
    {
        public Guid StudyTopicId { get; set; }

        public Guid DirectoryId { get; set; }

        public int Order { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; } = null!;

        [Required]
        public string PlanContent { get; set; } = null!;

        public Directory? Directory { get; set; }
        public StudyTopic? StudyTopic { get; set; }
        
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}