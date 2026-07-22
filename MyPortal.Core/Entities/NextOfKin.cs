using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Links a staff member to an emergency contact / next of kin. The contact itself is a
    // shared Contact record (a Person facet), mirroring StudentContactRelationship — so the
    // same person can be reused across relationships and carries their own phones/emails.
    [Table("NextOfKin")]
    public class NextOfKin : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid ContactId { get; set; }

        public Guid? RelationshipTypeId { get; set; }

        // Priority / order to call (1 = first).
        public int ContactOrder { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Contact? Contact { get; set; }
        public NextOfKinRelationshipType? RelationshipType { get; set; }

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
