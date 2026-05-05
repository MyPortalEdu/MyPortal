namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicYearUpsertRequest
{
    public AcademicYearUpsertRequest()
    {
        FirstWeekOffset = 0;
        AcademicTerms = Array.Empty<AcademicTermUpsertRequest>();
        AttendancePeriods = Array.Empty<AttendancePeriodUpsertRequest>();
        SchoolHolidays = Array.Empty<SchoolHolidayUpsertRequest>();
    }
    
    public int TimetableCycleLength { get; set; }
    public int SchoolWeekLength { get; set; }
    public int FirstWeekOffset { get; set; }
    public Guid? CopyPeriodsFromAcademicYearId { get; set; }
    
    public AcademicTermUpsertRequest[] AcademicTerms { get; set; }
    public AttendancePeriodUpsertRequest[] AttendancePeriods { get; set; }
    public SchoolHolidayUpsertRequest[] SchoolHolidays { get; set; }
}