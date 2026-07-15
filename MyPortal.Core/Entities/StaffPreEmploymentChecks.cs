using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // 1:1 summary record for a staff member's Single Central Record (SCR) checks that
    // are simple "done on this date" facts rather than their own record lists (DBS,
    // right-to-work, references and overseas checks each have their own tables). One
    // row per staff member; created lazily the first time the area is saved.
    [Table("StaffPreEmploymentChecks")]
    public class StaffPreEmploymentChecks : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public DateTime? IdentityCheckedDate { get; set; }

        public DateTime? ProhibitionFromTeachingCheckedDate { get; set; }

        // s128 direction check (prohibition from management of an independent school).
        public DateTime? ProhibitionFromManagementCheckedDate { get; set; }

        public DateTime? ChildcareDisqualificationCheckedDate { get; set; }

        public DateTime? MedicalFitnessCheckedDate { get; set; }

        public DateTime? QualificationsVerifiedDate { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }

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
