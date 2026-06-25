using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AddressPeople")]
    public class AddressPerson : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid AddressId { get; set; }

        public Guid? PersonId { get; set; }

        public Guid AddressTypeId { get; set; }

        public bool IsMain { get; set; }

        public bool IsDeleted { get; set; }

        public Address? Address { get; set; }
        public Person? Person { get; set; }
        public AddressType? AddressType { get; set; }

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