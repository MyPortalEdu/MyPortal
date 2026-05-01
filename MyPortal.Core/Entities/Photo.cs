using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Photos")]
    public class Photo : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid DocumentId { get; set; }

        public DateTime? PhotoDate { get; set; }

        public Document? Document { get; set; }

        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }

        // Migration 0009 added a Version BIGINT NOT NULL column on dbo.Photos. Without
        // IVersionedEntity here, EntityRepository.UpdateAsync skipped the optimistic-concurrency
        // path and Version was never bumped — silent lost-updates and a perma-stuck row version.
        public long Version { get; set; }
    }
}