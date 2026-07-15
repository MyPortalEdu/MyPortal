using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("RightToWorkChecks")]
    public class RightToWorkCheck : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid DocumentTypeId { get; set; }

        // The staff member who verified the documents — recorded server-side as the
        // current user where they are themselves staff; otherwise left null.
        public Guid? VerifiedById { get; set; }

        [StringLength(64)]
        public string? DocumentNumber { get; set; }

        public DateTime CheckDate { get; set; }

        public DateTime? DocumentExpiryDate { get; set; }

        // Date by which a follow-up check is required (e.g. visa-holders requiring re-check
        // before document expiry). Null = no follow-up required.
        public DateTime? FollowUpDate { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public RightToWorkDocumentType? DocumentType { get; set; }
        public StaffMember? VerifiedBy { get; set; }

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
