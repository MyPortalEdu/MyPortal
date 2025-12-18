using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;
using QueryKit.Repositories.Attributes;

namespace MyPortal.Core.Entities
{
    [Table("Documents")]
    public class Document : AuditableEntity, IDirectoryEntity, ISoftDeleteEntity
    {
        // Classification
        public Guid TypeId { get; set; }

        public Guid DirectoryId { get; set; }

        // Storage information
        [Required, StringLength(512)] 
        public string StorageKey { get; set; } = null!;

        [Required, StringLength(256)] 
        public string FileName { get; set; } = null!;

        [Required, StringLength(256)] 
        public string ContentType { get; set; } = null!;

        public long? SizeBytes { get; set; }

        [StringLength(128)]
        public string? Hash { get; set; }

        // User provided metadata
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
    }
}