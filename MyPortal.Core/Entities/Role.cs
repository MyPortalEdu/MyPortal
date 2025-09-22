using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Roles")]
    public class Role : Entity, ISystemEntity
    {
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        
        // Identity
        
        [StringLength(256)]
        public string? Name { get; set; }
        
        [StringLength(256)]
        public string? NormalizedName { get; set; }
        
        public string? ConcurrencyStamp { get; set; }
    }
}