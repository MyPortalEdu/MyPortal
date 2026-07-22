using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Vacancies")]
    public class Vacancy : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PostId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsAdvertised { get; set; }

        public bool IsTemporarilyFilled { get; set; }

        public Guid? SubjectId { get; set; }

        [StringLength(256)]
        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }

        public Post? Post { get; set; }
        public Subject? Subject { get; set; }

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
