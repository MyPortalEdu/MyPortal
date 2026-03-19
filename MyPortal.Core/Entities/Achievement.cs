using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Achievements")]
    public class Achievement : Entity, IAuditableEntity, ISoftDeleteEntity, IAcademicYearEntity
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
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
    }
}