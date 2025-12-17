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
        public string Description { get; set; } = null!;

        public bool IsSystem { get; set; }
    }
}