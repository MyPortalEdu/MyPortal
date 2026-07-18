using System.ComponentModel.DataAnnotations;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core
{
    public abstract class LookupEntity : Entity, ILookupEntity
    {
        [Required]
        [StringLength(256)] 
        public string Description { get; set; } = null!;

        public bool Active { get; set; } = true;
    }
}