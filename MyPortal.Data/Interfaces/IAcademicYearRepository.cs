using System.Data;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IAcademicYearRepository : IEntityRepository<AcademicYear>
{
    Task<DateTime?> GetEarliestTermStartDateAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task<bool> HasDownstreamDataAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task<bool> HasOverlapAsync(Guid? excludeAcademicYearId, DateTime rangeStart, DateTime rangeEnd,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task<IList<AcademicYearSummaryResponse>> GetSummariesAsync(CancellationToken cancellationToken);

    Task<AcademicYearDetailsResult?> GetDetailsByIdAsync(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<AcademicYearSummaryResponse?> GetCurrentAsync(CancellationToken cancellationToken);
}

public class AcademicYearDetailsResult
{
    public AcademicYearDetailsResponse Header { get; set; } = null!;
    public IList<AcademicTermResponse> Terms { get; set; } = new List<AcademicTermResponse>();
    public IList<SchoolHolidayRow> Holidays { get; set; } = new List<SchoolHolidayRow>();
    public IList<AttendancePeriodResponse> AttendancePeriods { get; set; }
        = new List<AttendancePeriodResponse>();
}

public class SchoolHolidayRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DiaryEventKind Kind { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
