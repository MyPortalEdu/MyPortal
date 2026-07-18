using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Auth.Cache;

public class RolePermissionCache(IMemoryCache cache) : IRolePermissionCache
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public Task<IReadOnlyCollection<string>?> GetAsync(Guid roleId, CancellationToken ct = default)
        => Task.FromResult(cache.Get<IReadOnlyCollection<string>>(Key(roleId)));

    public void Set(Guid roleId, IReadOnlyCollection<string> perms)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        options.RegisterPostEvictionCallback(OnEvicted, roleId);
        cache.Set(Key(roleId), perms, options);
    }

    private void OnEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        if (state is Guid roleId)
        {
            _locks.TryRemove(roleId, out _);
        }
    }

    public void Invalidate(Guid roleId) => cache.Remove(Key(roleId));

    public void InvalidateMany(IEnumerable<Guid> roleIds)
    {
        foreach (var r in roleIds) cache.Remove(Key(r));
    }

    public async Task<IReadOnlyCollection<string>> GetOrAddAsync(
        Guid roleId,
        Func<CancellationToken, Task<IReadOnlyCollection<string>>> factory,
        CancellationToken ct = default)
    {
        var cached = cache.Get<IReadOnlyCollection<string>>(Key(roleId));
        if (cached is not null) return cached;

        var sem = _locks.GetOrAdd(roleId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            // Double-check inside the lock — another caller may have populated the cache while we waited.
            cached = cache.Get<IReadOnlyCollection<string>>(Key(roleId));
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