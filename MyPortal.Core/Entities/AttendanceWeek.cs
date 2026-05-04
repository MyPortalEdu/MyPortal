using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceWeeks")]
    public class AttendanceWeek : Entity
    {
        public Guid AcademicTermId { get; set; }

        public DateTime Beginning { get; set; }

        // The cycle-day index that this calendar week's Monday lines up with. For a 5-day
        // pattern this is always 0; for a 10-day fortnightly pattern it alternates 0 / 5.
        public int CycleOffset { get; set; }

        public bool IsNonTimetable { get; set; }

        public AcademicTerm? AcademicTerm { get; set; }
    }
}
