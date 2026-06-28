using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractRepository : IEntityRepository<StaffContract>
{
    Task<IEnumerable<StaffContract>> GetByEmploymentIdsAsync(IEnumerable<Guid> employmentIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
