namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicYearUpsertRequest
{
    public int TimetableCycleLength { get; set; }
    public int SchoolWeekLength { get; set; }
    public int FirstWeekOffset { get; set; } = 0;
    public Guid? CopyPeriodsFromAcademicYearId { get; set; }
    public Guid? CopyPastoralStructureFromAcademicYearId { get; set; }
    
    public AcademicTermUpsertRequest[] AcademicTerms { get; set; } = Array.Empty<AcademicTermUpsertRequest>();
    public AttendancePeriodUpsertRequest[] AttendancePeriods { get; set; } = Array.Empty<AttendancePeriodUpsertRequest>();
    public SchoolHolidayUpsertRequest[] SchoolHolidays { get; set; } = Array.Empty<SchoolHolidayUpsertRequest>();
}