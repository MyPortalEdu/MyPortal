using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Directories")]
    public class Directory : Entity
    {
        public Guid? ParentId { get; set; }
        
        [Required, StringLength(128)]
        public required string Name { get; set; }

        // Only visible to staff users and the owner
        public bool IsPrivate { get; set; }

        public Directory? Parent { get; set; }
    }
}