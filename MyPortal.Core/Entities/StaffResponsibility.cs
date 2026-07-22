using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffResponsibilities")]
    public class StaffResponsibility : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid StaffMemberId { get; set; }

        public Guid ResponsibilityTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public StaffMember? StaffMember { get; set; }
        public StaffResponsibilityType? ResponsibilityType { get; set; }

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
