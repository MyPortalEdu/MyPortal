namespace MyPortal.Timetabler.Models;

public record Block(
    string Id,
    IReadOnlyList<int> SlotSizes,
    IReadOnlyList<Group> Groups,
    bool AllowsConcurrentScheduling = false);
