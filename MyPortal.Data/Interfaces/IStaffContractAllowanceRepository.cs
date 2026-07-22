using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractAllowanceRepository : IEntityRepository<StaffContractAllowance>
{
    /// <summary>
    /// Allowances for the given contracts (batched so the employment area loads the whole graph in
    /// one round trip). Soft-deleted rows are excluded.
    /// </summary>
    Task<IEnumerable<StaffContractAllowance>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
