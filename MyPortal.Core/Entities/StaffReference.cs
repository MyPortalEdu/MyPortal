using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // A pre-employment reference sought for a staff member. Hangs directly off the
    // StaffMember (a staff-level fact), mirroring DbsChecks / RightToWorkChecks.
    [Table("StaffReferences")]
    public class StaffReference : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid? ReferenceTypeId { get; set; }

        public Guid? ReferenceStatusId { get; set; }

        [Required]
        [StringLength(256)]
        public string RefereeName { get; set; } = null!;

        [StringLength(256)]
        public string? RefereeOrganisation { get; set; }

        [StringLength(256)]
        public string? RefereeEmail { get; set; }

        public DateTime? RequestedDate { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public ReferenceType? ReferenceType { get; set; }
        public ReferenceStatus? ReferenceStatus { get; set; }

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
