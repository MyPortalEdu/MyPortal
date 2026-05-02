namespace MyPortal.Timetabler.Models;

public record ClassDefinition(
    string Id,
    string SubjectId,
    IReadOnlyList<int> SessionSizes);
