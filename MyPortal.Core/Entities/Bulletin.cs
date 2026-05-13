using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Bulletins")]
    public class Bulletin : Entity, IAuditableEntity, IDirectoryEntity, IVersionedEntity
    {
        public Guid DirectoryId { get; set; }

        public Guid CategoryId { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Set when pinned, cleared when unpinned. Sortable so "newest pinned first"
        // is a single ORDER BY in feeds.
        public DateTime? PinnedAt { get; set; }

        [Required, StringLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        public string Detail { get; set; } = null!;

        public bool RequiresAcknowledgement { get; set; }

        public Directory? Directory { get; set; }
        public BulletinCategory? Category { get; set; }

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
