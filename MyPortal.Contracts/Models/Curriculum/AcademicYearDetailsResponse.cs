namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicYearDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsLocked { get; set; }

    public int TimetableCycleLength { get; set; }

    public int SchoolWeekLength { get; set; }

    public IList<AcademicTermResponse> Terms { get; set; } = new List<AcademicTermResponse>();

    public IList<SchoolHolidayResponse> SchoolHolidays { get; set; } = new List<SchoolHolidayResponse>();

    public IList<AttendancePeriodResponse> AttendancePeriods { get; set; }
        = new List<AttendancePeriodResponse>();
}
