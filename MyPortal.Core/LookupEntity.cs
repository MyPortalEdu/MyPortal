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
        public required string Description { get; set; }

        public bool Active { get; set; }
    }
}