using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ReportCardEntries")]
    public class ReportCardEntry : AuditableEntity
    {
        public Guid ReportCardId { get; set; }

        public Guid AttendanceWeekId { get; set; }

        public Guid AttendancePeriodId { get; set; }
        
        [StringLength(256)]
        public string? Comments { get; set; }

        public ReportCard? ReportCard { get; set; }
        public AttendanceWeek? AttendanceWeek { get; set; }
        public AttendancePeriod? AttendancePeriod { get; set; }
    }
}