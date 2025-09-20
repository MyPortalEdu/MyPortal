using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceWeeks")]
    public class AttendanceWeek : Entity
    {
        public Guid WeekPatternId { get; set; }

        public Guid AcademicTermId { get; set; }

        public DateTime Beginning { get; set; }

        public bool IsNonTimetable { get; set; }

        public AcademicTerm? AcademicTerm { get; set; }
        public AttendanceWeekPattern? WeekPattern { get; set; }
    }
}