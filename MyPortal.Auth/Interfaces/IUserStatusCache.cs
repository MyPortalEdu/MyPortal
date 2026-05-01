namespace MyPortal.Auth.Interfaces;

public interface IUserStatusCache
{
    /// <summary>
    /// Returns the cached IsEnabled state for the user, or invokes <paramref name="factory"/>
    /// under a per-key lock so concurrent callers don't all stampede the database.
    /// Cached entries are short-lived (~30s) so a disable propagates quickly across instances
    /// without per-permission-check DB hits. Call <see cref="Invalidate"/> to force a refresh
    /// on the same instance after an explicit user-state change.
    /// </summary>
    Task<bool> IsEnabledAsync(
        Guid userId,
        Func<CancellationToken, Task<bool>> factory,
        CancellationToken ct = default);

    void Invalidate(Guid userId);
}
