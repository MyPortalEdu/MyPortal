namespace MyPortal.Auth.Interfaces;

public interface IRolePermissionCache
{
    Task<IReadOnlyCollection<string>?> GetAsync(Guid roleId, CancellationToken ct = default);
    void Set(Guid roleId, IReadOnlyCollection<string> perms);
    void Invalidate(Guid roleId);
    void InvalidateMany(IEnumerable<Guid> roleIds);

    /// <summary>
    /// Returns the cached permissions for the role, or invokes <paramref name="factory"/> to
    /// produce them under a per-key lock so concurrent callers don't all stampede the source.
    /// </summary>
    Task<IReadOnlyCollection<string>> GetOrAddAsync(
        Guid roleId,
        Func<CancellationToken, Task<IReadOnlyCollection<string>>> factory,
        CancellationToken ct = default);
}