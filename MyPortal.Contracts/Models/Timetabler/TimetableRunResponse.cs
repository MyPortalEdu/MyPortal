namespace MyPortal.Contracts.Models.Timetabler;

public class TimetableRunResponse
{
    public Guid Id { get; set; }
    public Guid TimetableId { get; set; }
    public int Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? SolverDiagnostic { get; set; }
    public Guid TriggeredById { get; set; }
}
