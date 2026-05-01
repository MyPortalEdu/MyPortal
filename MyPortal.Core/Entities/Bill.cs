using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Bills")]
    public class Bill : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid StudentId { get; set; }

        public Guid? ChargeBillingPeriodId { get; set; }

        public DateTime DueDate { get; set; }

        public bool? IsDispatched { get; set; }

        public Student? Student { get; set; }

        public ChargeBillingPeriod? ChargeBillingPeriod { get; set; }
        
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