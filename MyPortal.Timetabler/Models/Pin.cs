namespace MyPortal.Timetabler.Models;

public record Pin(
    string BlockId,
    int SlotIndex,
    string? ClassId = null,
    string? TeacherId = null,
    string? RoomId = null,
    string? StartPeriodId = null);
