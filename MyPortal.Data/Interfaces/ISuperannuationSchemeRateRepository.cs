using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISuperannuationSchemeRateRepository : IEntityRepository<SuperannuationSchemeRate>
{
    /// <summary>The employer contribution rates in effect on a given date (one row per scheme).</summary>
    Task<IEnumerable<SuperannuationSchemeRate>> GetCurrentAsync(DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
