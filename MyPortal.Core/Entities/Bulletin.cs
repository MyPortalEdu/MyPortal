using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Bulletins")]
    public class Bulletin : Entity, IAuditableEntity, IDirectoryEntity, IVersionedEntity
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
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}