using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Extends a DiaryEvent (Kind = TrainingEvent); attendees are DiaryEventAttendees, completion → TrainingCertificate.
    [Table("TrainingEvents")]
    public class TrainingEvent : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid DiaryEventId { get; set; }

        public Guid TrainingCourseId { get; set; }

        [StringLength(200)]
        public string? Trainer { get; set; }

        [StringLength(200)]
        public string? Provider { get; set; }

        public decimal? Hours { get; set; }

        public int? Capacity { get; set; }

        public bool IsDeleted { get; set; }

        public DiaryEvent? DiaryEvent { get; set; }
        public TrainingCourse? TrainingCourse { get; set; }

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
