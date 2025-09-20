using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Documents")]
    public class Document : AuditableEntity, IDirectoryEntity, ISoftDeleteEntity
    {
        public Guid TypeId { get; set; }

        public Guid DirectoryId { get; set; }

        public Guid? FileId { get; set; }
        
        [Required, StringLength(128)]
        public string? Title { get; set; }
        
        [StringLength(256)]
        public string? Description { get; set; }

        // Only visible to staff users who have access to the directory
        public bool IsPrivate { get; set; }

        public bool IsDeleted { get; set; }

        public Directory? Directory { get; set; }

        public DocumentType? Type { get; set; }

        public File? Attachment { get; set; }
    }
}