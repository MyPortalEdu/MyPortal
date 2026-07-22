using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // A period an established post is unfilled. Open (null EndDate) vacancies on the census date are
    // what the School Workforce Return collects, along with whether they were advertised and
    // temporarily filled.
    [Table("Vacancies")]
    public class Vacancy : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PostId { get; set; }

        public DateTime StartDate { get; set; }

        // Null while still vacant; set when the post is filled or the vacancy is withdrawn.
        public DateTime? EndDate { get; set; }

        // Census: the vacancy has been advertised.
        public bool IsAdvertised { get; set; }

        // Census: covered by a temporary appointment while recruitment continues.
        public bool IsTemporarilyFilled { get; set; }

        // Census: the subject a teaching vacancy is for.
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
