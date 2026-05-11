using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AcademicYears")]
    public class AcademicYear : Entity, IAuditableEntity, IVersionedEntity
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;

        public bool IsLocked { get; set; }

        // Number of distinct days in the timetable cycle. 5 = standard weekly,
        // 10 = fortnightly (Week A / Week B), 6 = six-day rotation, etc.
        public int TimetableCycleLength { get; set; }

        // Number of school days in a calendar week. 5 = Mon-Fri, 6 = Mon-Sat. Together with
        // TimetableCycleLength this determines how the cycle advances week-to-week — offset
        // increments by SchoolWeekLength per calendar week, modulo TimetableCycleLength.
        // TimetableCycleLength must be a multiple of SchoolWeekLength.
        public int SchoolWeekLength { get; set; }
        
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