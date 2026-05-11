namespace MyPortal.Timetabler.Models;

public record Assignment(
    string BlockId,
    int SlotIndex,
    string ClassId,
    string TeacherId,
    string RoomId,
    IReadOnlyList<string> PeriodIds);
