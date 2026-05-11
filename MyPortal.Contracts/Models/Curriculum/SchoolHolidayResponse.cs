using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Curriculum;

public class SchoolHolidayResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    // Reverse-mapped from the underlying DiaryEvent.EventTypeId by the service —
    // the API surface mirrors the SchoolHolidayUpsertRequest shape used on create.
    public SchoolHolidayType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
