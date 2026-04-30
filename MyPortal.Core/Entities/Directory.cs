using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Directories")]
    public class Directory : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid? ParentId { get; set; }

        [Required, StringLength(128)] 
        public string Name { get; set; } = null!;

        // Only visible to staff users and the owner
        public bool IsPrivate { get; set; }

        public DirectoryUploadPolicy UploadPolicy { get; set; }

        public Directory? Parent { get; set; }
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}