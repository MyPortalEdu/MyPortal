using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IPhoneNumberRepository : IEntityRepository<PhoneNumber>
{
    /// <summary>
    /// All non-deleted phone numbers owned by a person, mains first. Entities (not DTOs) so the
    /// contact-details replace can diff and version-update them. Empty list if none.
    /// </summary>
    Task<IReadOnlyList<PhoneNumber>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
