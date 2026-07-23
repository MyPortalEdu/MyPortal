using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffEmployments")]
    public class StaffEmployment : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Carried in from earlier employment, so it may predate StartDate — it drives redundancy
        // and pension entitlement, not the length of this spell.
        public DateTime? ContinuousServiceStartDate { get; set; }

        public DateTime? LocalAuthorityStartDate { get; set; }

        public Guid? LeavingReasonId { get; set; }

        // Where the staff member was recruited from (CBDS CS055) — census arrivals.
        // Pairs with DestinationId: one origin/destination per employment spell.
        public Guid? OriginId { get; set; }

        // Destination on leaving (CBDS CS042) — workforce-census leavers return.
        public Guid? DestinationId { get; set; }

        [StringLength(128)]
        public string? PreviousEmployer { get; set; }

        [StringLength(128)]
        public string? NextEmployer { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public LeavingReason? LeavingReason { get; set; }
        public StaffOrigin? Origin { get; set; }
        public StaffDestination? Destination { get; set; }

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
