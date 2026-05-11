using System.Data;

namespace MyPortal.Data.Interfaces;

public interface IBulletinAcknowledgementRepository
{
    /// <summary>
    /// Records an acknowledgement of <paramref name="bulletinId"/> by <paramref name="userId"/>.
    /// Idempotent: re-acknowledging is a no-op. Returns true if a new row was inserted,
    /// false if the user had already acknowledged.
    /// </summary>
    Task<bool> AcknowledgeAsync(Guid bulletinId, Guid userId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
