namespace MyPortal.Timetabler.Models;

public record Teacher(
    string Id,
    IReadOnlyCollection<string> SubjectIds,
    int PpaPeriodsPerWeek = 0);
