using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceMarks")]
    public class AttendanceMark : AuditableEntity
    {
        public Guid StudentId { get; set; }

        public Guid AttendanceWeekId { get; set; }

        public Guid AttendancePeriodId { get; set; }

        public Guid AttendanceCodeId { get; set; }
        
        [StringLength(256)]
        public string? Comments { get; set; }
        
        [Range(1, int.MaxValue)]
        public int? MinutesLate { get; set; }

        public AttendanceCode? AttendanceCode { get; set; }

        public AttendancePeriod? AttendancePeriod { get; set; }

        public Student? Student { get; set; }

        public AttendanceWeek? AttendanceWeek { get; set; }
    }
}