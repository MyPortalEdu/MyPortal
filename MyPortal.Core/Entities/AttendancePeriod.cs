using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendancePeriods")]
    public class AttendancePeriod : Entity
    {
        public Guid WeekPatternId { get; set; }

        public DayOfWeek Weekday { get; set; }

        [Required, StringLength(128)]
        public string Name { get; set; } = null!;
        
        public TimeOnly StartTime { get; set; }
        
        public TimeOnly EndTime { get; set; }

        public bool IsAmReg { get; set; }

        public bool IsPmReg { get; set; }

        public AttendanceWeekPattern? WeekPattern { get; set; }
    }
}