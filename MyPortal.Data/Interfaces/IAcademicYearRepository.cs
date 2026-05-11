using System.Data;
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

    // Returns true if [rangeStart, rangeEnd] overlaps any AcademicTerm belonging to a
    // *different* academic year. Pass excludeAcademicYearId when updating; null on create.
    Task<bool> HasOverlapAsync(Guid? excludeAcademicYearId, DateTime rangeStart, DateTime rangeEnd,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task<IList<AcademicYearSummaryResponse>> GetSummariesAsync(CancellationToken cancellationToken);

    // Returns null when the AY doesn't exist (caller maps to 404). Holiday rows
    // include the raw DiaryEvent.EventTypeId; the service maps it to SchoolHolidayType.
    Task<AcademicYearDetailsResult?> GetDetailsByIdAsync(Guid academicYearId,
        CancellationToken cancellationToken);

    // Latest AY whose MIN(term.StartDate) <= today; null if no AY has started yet.
    Task<AcademicYearSummaryResponse?> GetCurrentAsync(CancellationToken cancellationToken);
}

// Internal carrier for the multi-result-set details read. The service flattens this
// into the public AcademicYearDetailsResponse contract after mapping the holiday type.
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
    public Guid EventTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
