using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IPersonDisabilityRepository : IEntityRepository<PersonDisability>
{
    Task<IEnumerable<PersonDisability>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
