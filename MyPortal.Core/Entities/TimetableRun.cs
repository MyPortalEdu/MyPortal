using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Enums;

namespace MyPortal.Core.Entities;

[Table("TimetableRuns")]
public class TimetableRun : Entity
{
    public Guid TimetableId { get; set; }

    public TimetableRunStatus Status { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? SolverDiagnostic { get; set; }

    public string? InputSnapshot { get; set; }

    public Guid TriggeredById { get; set; }

    public Timetable? Timetable { get; set; }
    public User? TriggeredBy { get; set; }
}
