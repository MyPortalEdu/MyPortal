using System.Data;
using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISchoolRepository : IEntityRepository<Core.Entities.School>
{
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsResponse?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken);

    /// <summary>The local school's default pay zone, or null if unset. Drives statutory salary
    /// lookups for staff contracts.</summary>
    Task<Guid?> GetLocalSchoolPayZoneIdAsync(CancellationToken cancellationToken);

    /// <summary>True if another school already uses this URN. Pass <paramref name="excludeSchoolId"/>
    /// on update so a school doesn't clash with itself.</summary>
    Task<bool> UrnExistsAsync(string urn, Guid? excludeSchoolId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}