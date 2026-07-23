using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractSalaryChangeRepository : IEntityRepository<StaffContractSalaryChange>
{
    Task<IEnumerable<StaffContractSalaryChangeRow>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Of the given contracts, those already incremented for <paramref name="effectiveDate"/> — so a
    /// cycle isn't applied twice by a re-run or a scheduled + manual overlap.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetIncrementedContractIdsAsync(IEnumerable<Guid> contractIds,
        DateTime effectiveDate, CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
