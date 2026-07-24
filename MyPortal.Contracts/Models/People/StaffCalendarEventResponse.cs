namespace MyPortal.Contracts.Models.People;

public class StaffCalendarEventResponse
{
    public string Id { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public bool AllDay { get; set; }

    public string Category { get; set; } = null!;

    public string? Location { get; set; }

    public byte? Kind { get; set; }

    public string? ColourCode { get; set; }
}
