using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendancePeriods")]
    public class AttendancePeriod : Entity
    {
        public Guid AcademicYearId { get; set; }

        // Position within the AY's cycle (0 .. AcademicYear.TimetableCycleLength - 1).
        // For a 5-day cycle this maps onto Mon..Fri; for a 10-day fortnightly cycle,
        // 0..4 are Week A and 5..9 are Week B. The calendar weekday is computed at
        // materialisation time using the owning AttendanceWeek's CycleOffset.
        public int CycleDayIndex { get; set; }

        [Required, StringLength(128)]
        public string Name { get; set; } = null!;

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsAmReg { get; set; }

        public bool IsPmReg { get; set; }

        public AcademicYear? AcademicYear { get; set; }
    }
}
