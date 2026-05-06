using System.Data;
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IAcademicYearRepository : IBaseEntityRepository<AcademicYear, Guid>
{
    Task<DateTime?> GetEarliestTermStartDateAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task<bool> HasDownstreamDataAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    // Returns true if [rangeStart, rangeEnd] overlaps any AcademicTerm belonging to a
    // *different* academic year. Pass excludeAcademicYearId when updating; null on create.
    Task<bool> HasOverlapAsync(Guid? excludeAcademicYearId, DateTime rangeStart, DateTime rangeEnd,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
