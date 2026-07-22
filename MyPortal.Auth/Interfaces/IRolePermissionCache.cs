namespace MyPortal.Auth.Interfaces;

public interface IRolePermissionCache
{
    Task<IReadOnlyCollection<string>?> GetAsync(Guid roleId, CancellationToken ct = default);
    void Set(Guid roleId, IReadOnlyCollection<string> perms);
    void Invalidate(Guid roleId);
    void InvalidateMany(IEnumerable<Guid> roleIds);
    
    Task<IReadOnlyCollection<string>> GetOrAddAsync(
        Guid roleId,
        Func<CancellationToken, Task<IReadOnlyCollection<string>>> factory,
        CancellationToken ct = default);
}