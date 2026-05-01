using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Auth.Cache;

public class UserStatusCache : IUserStatusCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public UserStatusCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(
        Guid userId,
        Func<CancellationToken, Task<bool>> factory,
        CancellationToken ct = default)
    {
        if (_cache.TryGetValue<bool>(Key(userId), out var cached))
        {
            return cached;
        }

        var sem = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            if (_cache.TryGetValue(Key(userId), out cached))
            {
                return cached;
            }

            var fresh = await factory(ct);
            _cache.Set(Key(userId), fresh, Ttl);
            return fresh;
        }
        finally
        {
            sem.Release();
        }
    }

    public void Invalidate(Guid userId) => _cache.Remove(Key(userId));

    private static string Key(Guid userId) => $"user:enabled:{userId}";
}
