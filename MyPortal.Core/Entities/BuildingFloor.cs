using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BuildingFloors")]
    public class BuildingFloor : LookupEntity
    {
        public Guid BuildingId { get; set; }

        public Building? Building { get; set; }
    }
}