using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Rooms")]
    public class Room : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid? BuildingFloorId { get; set; }

        [StringLength(10)] 
        public string? Code { get; set; }

        [Required] 
        [StringLength(256)] 
        public string Name { get; set; } = null!;

        public int MaxGroupSize { get; set; }

        public string? TelephoneNo { get; set; }

        public bool IsExcludedFromCover { get; set; }

        public BuildingFloor? BuildingFloor { get; set; }
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}