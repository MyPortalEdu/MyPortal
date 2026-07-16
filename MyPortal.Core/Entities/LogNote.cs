using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("LogNotes")]
    public class LogNote : Entity, IAuditableEntity, ISoftDeleteEntity, IAcademicYearEntity, IVersionedEntity
    {
        public Guid LogNoteTypeId { get; set; }

        public Guid StudentId { get; set; }

        public Guid AcademicYearId { get; set; }

        [Required]
        public string Message { get; set; } = null!;

        // Only visible to staff users
        public bool IsPrivate { get; set; }

        public bool IsDeleted { get; set; }

        public Student? Student { get; set; }

        public AcademicYear? AcademicYear { get; set; }

        public LogNoteType? LogNoteType { get; set; }
        
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