using Microsoft.Extensions.Caching.Memory;
using MyPortal.Data.Interfaces;

namespace MyPortal.Data.Security;

public class RolePermissionCache : IRolePermissionCache
{
    private readonly IMemoryCache _cache;
    public RolePermissionCache(IMemoryCache cache) => _cache = cache;

    public Task<IReadOnlyCollection<string>> GetAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult(_cache.Get<IReadOnlyCollection<string>>(Key(roleId)) ?? Array.Empty<string>());

    public void Set(Guid roleId, IReadOnlyCollection<string> perms)
        => _cache.Set(Key(roleId), perms, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });

    public void Invalidate(Guid roleId) => _cache.Remove(Key(roleId));

    public void InvalidateMany(IEnumerable<Guid> roleIds)
    {
        foreach (var r in roleIds) _cache.Remove(Key(r));
    }

    private static string Key(Guid roleId) => $"perms:role:{roleId}";
}