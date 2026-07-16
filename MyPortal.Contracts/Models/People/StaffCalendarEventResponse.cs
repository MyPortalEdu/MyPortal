namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single dated entry on a staff member's timetable/calendar. Sourced from several places
/// (lessons, detentions, diary events, non-contact periods, parent-evening appointments,
/// absences) and flattened to one shape for the calendar to render. Read-only.
/// </summary>
public class StaffCalendarEventResponse
{
    /// <summary>
    /// A stable, unique id for the entry. Not always a single entity id — a lesson instance is
    /// keyed by (session, period, week), so this is a string rather than a Guid.
    /// </summary>
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public bool AllDay { get; set; }

    /// <summary>
    /// The source bucket, driving the calendar's colour/legend: Lesson, Cover, Detention, Event,
    /// Holiday, NonContact, ParentEvening, Absence.
    /// </summary>
    public string Category { get; set; } = null!;

    public string? Location { get; set; }

    /// <summary>An explicit colour from the diary event type, when the source carries one.</summary>
    public string? ColourCode { get; set; }
}
