namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Read payload for the Timetable area: every calendar entry overlapping the requested window
/// that the viewer is permitted to see, ordered by start time. The window is supplied by the
/// caller (the calendar's visible range).
/// </summary>
public class StaffCalendarResponse
{
    public List<StaffCalendarEventResponse> Events { get; set; } = [];
}
