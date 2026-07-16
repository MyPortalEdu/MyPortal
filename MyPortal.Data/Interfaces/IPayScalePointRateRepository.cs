using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IPayScalePointRateRepository : IEntityRepository<PayScalePointRate>
{
    /// <summary>
    /// The statutory rates in effect for a pay zone on a given date (one row per pay-scale
    /// point). Used to surface the full-time salary for each spine point in the editor.
    /// </summary>
    Task<IEnumerable<PayScalePointRate>> GetCurrentByZoneAsync(Guid payZoneId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
