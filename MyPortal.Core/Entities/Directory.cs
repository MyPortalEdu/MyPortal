using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;
using MyPortal.Core.Interfaces;
using QueryKit.Repositories.Attributes;

namespace MyPortal.Core.Entities
{
    [Table("Directories")]
    public class Directory : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid? ParentId { get; set; }

        [Required, StringLength(128)]
        public string Name { get; set; } = null!;

        // Only visible to staff users and the owner
        public bool IsPrivate { get; set; }

        public DirectoryUploadPolicy UploadPolicy { get; set; }

        // Soft-delete so a directory delete keeps the row (recoverable) and doesn't trip the
        // Documents.DirectoryId FK against documents that were already soft-deleted inside it.
        [SoftDelete]
        public bool IsDeleted { get; set; }

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