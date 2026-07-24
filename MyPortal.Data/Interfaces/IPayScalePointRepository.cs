using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IPayScalePointRepository : IEntityRepository<PayScalePoint>
{
    Task<IEnumerable<PayScaleUsageRow>> GetContractCountsAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
