using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;
using QueryKit.Repositories.Attributes;

namespace MyPortal.Core.Entities
{
    [Table("Documents")]
    public class Document : Entity, IAuditableEntity, IDirectoryEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid TypeId { get; set; }

        public Guid DirectoryId { get; set; }

        [Required, StringLength(512)] 
        public string StorageKey { get; set; } = null!;

        [Required, StringLength(256)] 
        public string FileName { get; set; } = null!;

        [Required, StringLength(256)] 
        public string ContentType { get; set; } = null!;

        public long? SizeBytes { get; set; }

        [StringLength(128)]
        public string? Hash { get; set; }

        [Required, StringLength(128)]
        public string? Title { get; set; }
        
        [StringLength(256)]
        public string? Description { get; set; }

        // Only visible to staff users who have access to the directory
        public bool IsPrivate { get; set; }

        [SoftDelete]
        public bool IsDeleted { get; set; }

        public Directory? Directory { get; set; }

        public DocumentType? Type { get; set; }
        
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