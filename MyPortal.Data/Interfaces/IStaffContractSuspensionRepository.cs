using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractSuspensionRepository : IEntityRepository<StaffContractSuspension>
{
    /// <summary>Suspensions for the given contracts. Soft-deleted rows are excluded.</summary>
    Task<IEnumerable<StaffContractSuspension>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
