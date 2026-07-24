using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISuperannuationSchemeRateRepository : IEntityRepository<SuperannuationSchemeRate>
{
    Task<IEnumerable<SuperannuationSchemeRate>> GetCurrentAsync(DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
