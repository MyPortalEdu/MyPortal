using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractRepository : IEntityRepository<StaffContract>
{
    Task<IEnumerable<StaffContract>> GetByEmploymentIdsAsync(IEnumerable<Guid> employmentIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Live contracts on a service term that hold a pay-scale point, as of <paramref name="asOf"/> —
    /// the candidates for the annual increment routine.
    /// </summary>
    Task<IReadOnlyList<IncrementCandidateRow>> GetIncrementCandidatesAsync(Guid serviceTermId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
