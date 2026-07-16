using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IAddressPersonRepository : IEntityRepository<AddressPerson>
{
    /// <summary>
    /// A person's non-deleted address links (entities), for reconciling the single-main rule.
    /// </summary>
    Task<IReadOnlyList<AddressPerson>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
