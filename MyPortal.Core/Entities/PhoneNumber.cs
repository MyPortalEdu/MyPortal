using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("PhoneNumbers")]
    public class PhoneNumber : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid TypeId { get; set; }

        public Guid? PersonId { get; set; }

        public Guid? AgencyId { get; set; }

        [Phone]
        [Required]
        [StringLength(128)]
        public string Number { get; set; } = null!;

        public bool IsMain { get; set; }

        public bool IsDeleted { get; set; }

        public PhoneNumberType? Type { get; set; }
        public Person? Person { get; set; }
        public Agency? Agency { get; set; }

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