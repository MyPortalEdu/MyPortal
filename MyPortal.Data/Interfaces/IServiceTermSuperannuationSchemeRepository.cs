using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IServiceTermSuperannuationSchemeRepository : IEntityRepository<ServiceTermSuperannuationScheme>
{
    Task<IEnumerable<ServiceTermSuperannuationScheme>> GetByServiceTermIdsAsync(IEnumerable<Guid> serviceTermIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
