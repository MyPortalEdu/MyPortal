namespace MyPortal.Data.Interfaces;

public interface IRolePermissionCache
{
    Task<IReadOnlyCollection<string>> GetAsync(Guid roleId, CancellationToken ct = default);
    void Set(Guid roleId, IReadOnlyCollection<string> perms);
    void Invalidate(Guid roleId);
    void InvalidateMany(IEnumerable<Guid> roleIds);
}