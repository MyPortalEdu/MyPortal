using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Directories")]
    public class Directory : Entity
    {
        public Guid? ParentId { get; set; }

        [Required, StringLength(128)] 
        public string Name { get; set; } = null!;

        // Only visible to staff users and the owner
        public bool IsPrivate { get; set; }

        public Directory? Parent { get; set; }
    }
}