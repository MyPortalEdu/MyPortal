using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractAllowanceRepository : IEntityRepository<StaffContractAllowance>
{
    Task<IEnumerable<StaffContractAllowance>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
