namespace MyPortal.Timetabler.Models;

public record PeriodSlot(
    string Id,
    int Day,
    int OrderInDay,
    bool NoDoubleAfter);
