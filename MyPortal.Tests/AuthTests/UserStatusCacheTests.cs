using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Cache;

namespace MyPortal.Tests.AuthTests;

[TestFixture]
public class UserStatusCacheTests
{
    private MemoryCache _memoryCache = null!;
    private UserStatusCache _cache = null!;

    [SetUp]
    public void Setup()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cache = new UserStatusCache(_memoryCache);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [Test]
    public async Task IsEnabledAsync_CacheMiss_InvokesFactory_AndStoresResult()
    {
        var userId = Guid.NewGuid();
        var calls = 0;

        var first = await _cache.IsEnabledAsync(userId, _ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(true);
        });
        var second = await _cache.IsEnabledAsync(userId, _ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(true);
        });

        Assert.That(first, Is.True);
        Assert.That(second, Is.True);
        Assert.That(calls, Is.EqualTo(1), "Factory should run once; subsequent calls hit the cache.");
    }

    [Test]
    public async Task IsEnabledAsync_CachesFalseResults_TooButCanBeOverturnedByInvalidate()
    {
        var userId = Guid.NewGuid();
        var factoryAnswer = false;

        var disabled = await _cache.IsEnabledAsync(userId, _ => Task.FromResult(factoryAnswer));
        Assert.That(disabled, Is.False);

        // Without invalidation, the cached `false` is returned even if the factory now flips.
        factoryAnswer = true;
        var stillDisabled = await _cache.IsEnabledAsync(userId, _ => Task.FromResult(factoryAnswer));
        Assert.That(stillDisabled, Is.False);

        // Invalidate forces a re-check.
        _cache.Invalidate(userId);
        var nowEnabled = await _cache.IsEnabledAsync(userId, _ => Task.FromResult(factoryAnswer));
        Assert.That(nowEnabled, Is.True);
    }

    [Test]
    public async Task IsEnabledAsync_ConcurrentCallersForSameKey_OnlyInvokeFactoryOnce()
    {
        var userId = Guid.NewGuid();
        var factoryEntered = 0;
        var release = new TaskCompletionSource();

        async Task<bool> Factory(CancellationToken _)
        {
            Interlocked.Increment(ref factoryEntered);
            // Hold the first caller inside the factory until we've launched all callers,
            // so the per-key SemaphoreSlim has to serialize the rest.
            await release.Task;
            return true;
        }

        var t1 = _cache.IsEnabledAsync(userId, Factory);
        var t2 = _cache.IsEnabledAsync(userId, Factory);
        var t3 = _cache.IsEnabledAsync(userId, Factory);

        // Give all three a chance to reach the lock.
        await Task.Delay(50);
        release.SetResult();

        await Task.WhenAll(t1, t2, t3);

        Assert.That(factoryEntered, Is.EqualTo(1),
            "Per-key lock should serialize and the double-check should let the latecomers see the cached value.");
    }

    [Test]
    public async Task IsEnabledAsync_DifferentKeys_RunFactoriesInParallel()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var bothEntered = new SemaphoreSlim(0, 2);
        var release = new TaskCompletionSource();

        async Task<bool> Factory(CancellationToken _)
        {
            bothEntered.Release();
            await release.Task;
            return true;
        }

        var t1 = _cache.IsEnabledAsync(keyA, Factory);
        var t2 = _cache.IsEnabledAsync(keyB, Factory);

        // If different keys shared a lock, only one factory would have entered before release.
        var firstEntered = await bothEntered.WaitAsync(TimeSpan.FromSeconds(2));
        var secondEntered = await bothEntered.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.That(firstEntered, Is.True);
        Assert.That(secondEntered, Is.True, "Different keys should not share a lock.");

        release.SetResult();
        await Task.WhenAll(t1, t2);
    }
}
