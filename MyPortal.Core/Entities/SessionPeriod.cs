using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("SessionPeriods")]
public class SessionPeriod : Entity
{
    public Guid SessionId { get; set; }
    public Guid PeriodId { get; set; }

    public Session? Session { get; set; }
    public AttendancePeriod? Period { get; set; }
}