namespace MyPortal.Timetabler.Models;

public record Group(string Id, IReadOnlyList<ClassDefinition> Classes);
