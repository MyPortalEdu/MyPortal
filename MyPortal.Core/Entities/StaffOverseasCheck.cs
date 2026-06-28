using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // An overseas check for a staff member who has lived/worked abroad (KCSIE: a
    // "certificate of good conduct" or equivalent per country). Country reuses the
    // Nationality lookup. Hangs directly off the StaffMember.
    [Table("StaffOverseasChecks")]
    public class StaffOverseasCheck : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        // Country the check relates to — reuses the Nationalities lookup.
        public Guid NationalityId { get; set; }

        public DateTime? CheckedDate { get; set; }

        // Whether the check came back clear / satisfactory.
        public bool IsClear { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Nationality? Nationality { get; set; }

        // Audit
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
