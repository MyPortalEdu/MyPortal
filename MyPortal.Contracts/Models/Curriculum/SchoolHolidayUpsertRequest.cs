using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Curriculum;

public class SchoolHolidayUpsertRequest
{
    public Guid? SchoolHolidayId { get; set; }
    public string Name { get; set; } = null!;
    public SchoolHolidayType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}