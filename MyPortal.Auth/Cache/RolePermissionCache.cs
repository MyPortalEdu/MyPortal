using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Auth.Cache;

public class RolePermissionCache : IRolePermissionCache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public RolePermissionCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<IReadOnlyCollection<string>?> GetAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult(_cache.Get<IReadOnlyCollection<string>>(Key(roleId)));

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

    public async Task<IReadOnlyCollection<string>> GetOrAddAsync(
        Guid roleId,
        Func<CancellationToken, Task<IReadOnlyCollection<string>>> factory,
        CancellationToken ct = default)
    {
        var cached = _cache.Get<IReadOnlyCollection<string>>(Key(roleId));
        if (cached is not null) return cached;

        var sem = _locks.GetOrAdd(roleId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            // Double-check inside the lock — another caller may have populated the cache while we waited.
            cached = _cache.Get<IReadOnlyCollection<string>>(Key(roleId));
            if (cached is not null) return cached;

            var fresh = await factory(ct);
            Set(roleId, fresh);
            return fresh;
        }
        finally
        {
            sem.Release();
        }
    }

    private static string Key(Guid roleId) => $"perms:role:{roleId}";
}