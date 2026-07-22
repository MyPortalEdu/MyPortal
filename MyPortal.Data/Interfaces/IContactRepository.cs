using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IContactRepository : IEntityRepository<Contact>
{
    /// <summary>The Contact facet for a person, if one exists (a person is a contact at most once).</summary>
    Task<Contact?> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
