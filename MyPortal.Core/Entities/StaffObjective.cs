using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // An appraisal objective / target for a staff member (1:many). Soft-deleted on reconcile so
    // the audit history of past objectives is preserved.
    [Table("StaffObjectives")]
    public class StaffObjective : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid? ReviewId { get; set; }

        public Guid? CategoryId { get; set; }

        [Required]
        [StringLength(256)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public string? SuccessCriteria { get; set; }

        public DateTime? DueDate { get; set; }

        public Guid? StatusId { get; set; }

        public string? ProgressNotes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public PerformanceReview? Review { get; set; }
        public ObjectiveCategory? Category { get; set; }
        public ObjectiveStatus? Status { get; set; }

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
