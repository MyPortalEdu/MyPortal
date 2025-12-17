using System.ComponentModel.DataAnnotations;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core
{
    public abstract class LookupEntity : Entity, ILookupEntity
    {
        public LookupEntity()
        {
            Active = true;
        }

        [Required]
        [StringLength(256)] 
        public string Description { get; set; } = null!;

        public bool Active { get; set; }
    }
}