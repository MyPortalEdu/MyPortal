using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Contacts")]
    public class Contact : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PersonId { get; set; }

        public bool ParentalBallot { get; set; }

        [StringLength(256)]
        public string? PlaceOfWork { get; set; }

        [StringLength(256)]
        public string? JobTitle { get; set; }

        [StringLength(128)]
        public string? NiNumber { get; set; }

        public bool IsDeleted { get; set; }

        public Person? Person { get; set; }

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
