namespace MyPortal.Timetabler.Models;

public record Room(string Id, IReadOnlyCollection<string> SubjectIds);
