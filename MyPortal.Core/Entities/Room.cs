using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Rooms")]
    public class Room : Entity
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
    }
}