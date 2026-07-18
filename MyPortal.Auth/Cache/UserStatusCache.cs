using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Auth.Cache;

public class UserStatusCache(IMemoryCache cache) : IUserStatusCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public async Task<bool> IsEnabledAsync(
        Guid userId,
        Func<CancellationToken, Task<bool>> factory,
        CancellationToken ct = default)
    {
        if (cache.TryGetValue<bool>(Key(userId), out var cached))
        {
            return cached;
        }

        var sem = _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            if (cache.TryGetValue(Key(userId), out cached))
            {
                return cached;
            }

            var fresh = await factory(ct);
            cache.Set(Key(userId), fresh, Ttl);
            return fresh;
        }
        finally
        {
            sem.Release();
        }
    }

    public void Invalidate(Guid userId) => cache.Remove(Key(userId));

    private static string Key(Guid userId) => $"user:enabled:{userId}";
}
