namespace MyPortal.Timetabler.Models;

public record TimetableInput(
    IReadOnlyList<PeriodSlot> Periods,
    IReadOnlyList<Teacher> Teachers,
    IReadOnlyList<Room> Rooms,
    IReadOnlyList<Band> Bands,
    IReadOnlyList<Block> Blocks,
    IReadOnlyList<Pin> Pins);
