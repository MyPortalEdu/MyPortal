using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Locations")]
    public class Location : Entity, ISystemEntity
    {
        [Required]
        [StringLength(128)]
        public required string Description { get; set; }

        public bool IsSystem { get; set; }
    }
}