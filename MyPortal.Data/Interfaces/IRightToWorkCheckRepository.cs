using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IRightToWorkCheckRepository : IEntityRepository<RightToWorkCheck>
{
    Task<IEnumerable<RightToWorkCheck>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
