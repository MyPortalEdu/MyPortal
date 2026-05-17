using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Scaffold = QueryKit.Attributes.ScaffoldAttribute;

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

        // [Scaffold] is required because QueryKit's IsSimpleType allow-list omits
        // TimeOnly/DateOnly — without it, GetScaffoldableProperties filters these
        // columns out of the generated INSERT/UPDATE, sending NULL to a NOT NULL
        // TIME column. The Dapper TimeOnlyTypeHandler handles the value binding
        // once the column is included; [Scaffold] is what gets it included.
        [Scaffold]
        public TimeOnly StartTime { get; set; }

        [Scaffold]
        public TimeOnly EndTime { get; set; }

        public bool IsAmReg { get; set; }

        public bool IsPmReg { get; set; }

        // True when the period is taught (feeds the lesson/Sessions pipeline). Can be
        // combined with IsAmReg/IsPmReg so that a single lesson doubles as that day's
        // registration session — taken by the subject teacher rather than the form
        // tutor. A period with IsLesson=false and a reg flag is a pure form-time slot.
        public bool IsLesson { get; set; }

        public AcademicYear? AcademicYear { get; set; }
    }
}
