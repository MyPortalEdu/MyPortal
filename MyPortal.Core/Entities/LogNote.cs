using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("LogNotes")]
    public class LogNote : AuditableEntity, ISoftDeleteEntity
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
    }
}