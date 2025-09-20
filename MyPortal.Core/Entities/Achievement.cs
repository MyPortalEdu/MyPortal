using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Achievements")]
    public class Achievement : AuditableEntity, ISoftDeleteEntity
    {
        public Guid AcademicYearId { get; set; }

        public Guid AchievementTypeId { get; set; }

        public Guid? LocationId { get; set; }

        public DateTime Date { get; set; }

        public string? Comments { get; set; }

        public bool IsDeleted { get; set; }

        public AchievementType? Type { get; set; }

        public Location? Location { get; set; }

        public AcademicYear? AcademicYear { get; set; }
    }
}