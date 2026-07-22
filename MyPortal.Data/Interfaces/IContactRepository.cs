using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IContactRepository : IEntityRepository<Contact>
{
    Task<Contact?> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
