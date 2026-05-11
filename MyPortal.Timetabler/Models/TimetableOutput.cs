namespace MyPortal.Timetabler.Models;

public record TimetableOutput(
    SolveStatus Status,
    IReadOnlyList<Assignment> Assignments,
    string? Diagnostic = null);
