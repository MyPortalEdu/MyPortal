using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("SessionExtraNames")]
public class SessionExtraName : Entity
{
    public Guid AttendanceWeekId { get; set; }
    public Guid SessionId { get; set; }
    public Guid StudentId { get; set; }

    public AttendanceWeek? AttendanceWeek { get; set; }
    public Session? Session { get; set; }
    public Student? Student { get; set; }
}