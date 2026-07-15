using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // An appraisal review cycle for a staff member (1:many — one row per annual cycle, so the
    // year-on-year history is preserved). The overall rating reuses the ObservationOutcomes scale.
    [Table("PerformanceReviews")]
    public class PerformanceReview : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        [StringLength(128)]
        public string? CycleName { get; set; }

        public Guid? ReviewerId { get; set; }

        public Guid? StatusId { get; set; }

        public DateTime? ReviewDate { get; set; }

        public DateTime? NextReviewDate { get; set; }

        // → ObservationOutcomes (shared rating scale).
        public Guid? OverallOutcomeId { get; set; }

        public string? Summary { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public StaffMember? Reviewer { get; set; }
        public ReviewStatus? Status { get; set; }
        public ObservationOutcome? OverallOutcome { get; set; }

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
