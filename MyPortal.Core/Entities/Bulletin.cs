using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Bulletins")]
    public class Bulletin : AuditableEntity, IDirectoryEntity
    {
        public Guid DirectoryId { get; set; }

        public DateTime? ExpiresAt { get; set; }

        [Required, StringLength(50)] 
        public string Title { get; set; } = null!;

        [Required] 
        public string Detail { get; set; } = null!;

        public bool IsPrivate { get; set; }

        public bool IsApproved { get; set; }
        
        public Directory? Directory { get; set; }
    }
}