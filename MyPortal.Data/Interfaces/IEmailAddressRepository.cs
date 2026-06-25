using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IEmailAddressRepository : IEntityRepository<EmailAddress>
{
    /// <summary>
    /// All non-deleted email addresses owned by a person, mains first. Entities (not DTOs) so the
    /// contact-details replace can diff and version-update them. Empty list if none.
    /// </summary>
    Task<IReadOnlyList<EmailAddress>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
