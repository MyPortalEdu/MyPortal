using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ReportCardEntries")]
    public class ReportCardEntry : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid ReportCardId { get; set; }

        public Guid AttendanceWeekId { get; set; }

        public Guid AttendancePeriodId { get; set; }
        
        [StringLength(256)]
        public string? Comments { get; set; }

        public ReportCard? ReportCard { get; set; }
        public AttendanceWeek? AttendanceWeek { get; set; }
        public AttendancePeriod? AttendancePeriod { get; set; }
        
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