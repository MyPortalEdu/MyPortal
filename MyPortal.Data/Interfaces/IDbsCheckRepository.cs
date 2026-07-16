using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IDbsCheckRepository : IEntityRepository<DbsCheck>
{
    Task<IEnumerable<DbsCheck>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
