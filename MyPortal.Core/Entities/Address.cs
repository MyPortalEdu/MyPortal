using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Addresses")]
    public class Address : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        [StringLength(128)]
        public string? BuildingNumber { get; set; }
        
        [StringLength(128)]
        public string? BuildingName { get; set; }
        
        [StringLength(128)]
        public string? Apartment { get; set; }

        [Required]
        [StringLength(256)]
        public string Street { get; set; } = null!;
        
        [StringLength(256)]
        public string? District { get; set; }

        [Required, StringLength(256)]
        public string Town { get; set; } = null!;

        [Required, StringLength(256)]
        public string County { get; set; } = null!;

        [Required, StringLength(128)]
        public string Postcode { get; set; } = null!;

        [Required, StringLength(128)]
        public string Country { get; set; } = null!;

        public bool IsValidated { get; set; }

        public bool IsDeleted { get; set; }

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