using Microsoft.Extensions.Caching.Memory;
using MyPortal.Auth.Cache;

namespace MyPortal.Tests.AuthTests;

[TestFixture]
public class RolePermissionCacheTests
{
    private MemoryCache _memoryCache = null!;
    private RolePermissionCache _cache = null!;

    [SetUp]
    public void Setup()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cache = new RolePermissionCache(_memoryCache);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [Test]
    public async Task GetAsync_ReturnsNull_WhenNotCached()
    {
        var result = await _cache.GetAsync(Guid.NewGuid());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SetAndGet_RoundTrip()
    {
        var roleId = Guid.NewGuid();
        var perms = new[] { "A", "B" };

        _cache.Set(roleId, perms);
        var result = await _cache.GetAsync(roleId);

        Assert.That(result, Is.EquivalentTo(perms));
    }

    [Test]
    public void Invalidate_RemovesCachedValue()
    {
        var roleId = Guid.NewGuid();
        _cache.Set(roleId, new[] { "A" });

        _cache.Invalidate(roleId);

        Assert.That(_cache.GetAsync(roleId).Result, Is.Null);
    }

    [Test]
    public void InvalidateMany_RemovesEachValue()
    {
        var keep = Guid.NewGuid();
        var drop1 = Guid.NewGuid();
        var drop2 = Guid.NewGuid();

        _cache.Set(keep, new[] { "K" });
        _cache.Set(drop1, new[] { "1" });
        _cache.Set(drop2, new[] { "2" });

        _cache.InvalidateMany(new[] { drop1, drop2 });

        Assert.That(_cache.GetAsync(keep).Result, Is.Not.Null);
        Assert.That(_cache.GetAsync(drop1).Result, Is.Null);
        Assert.That(_cache.GetAsync(drop2).Result, Is.Null);
    }

    [Test]
    public async Task GetOrAddAsync_CacheMiss_InvokesFactory_AndStoresResult()
    {
        var roleId = Guid.NewGuid();
        var calls = 0;

        var first = await _cache.GetOrAddAsync(roleId, _ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult<IReadOnlyCollection<string>>(new[] { "A" });
        });
        var second = await _cache.GetOrAddAsync(roleId, _ =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult<IReadOnlyCollection<string>>(new[] { "B" });
        });

        Assert.That(first, Is.EquivalentTo(new[] { "A" }));
        Assert.That(second, Is.EquivalentTo(new[] { "A" }), "Second call should hit cache.");
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public async Task GetOrAddAsync_ConcurrentCallersForSameKey_OnlyInvokeFactoryOnce()
    {
        var roleId = Guid.NewGuid();
        var factoryEntered = 0;
        var release = new TaskCompletionSource();

        async Task<IReadOnlyCollection<string>> Factory(CancellationToken _)
        {
            Interlocked.Increment(ref factoryEntered);
            await release.Task;
            return new[] { "X" };
        }

        var t1 = _cache.GetOrAddAsync(roleId, Factory);
        var t2 = _cache.GetOrAddAsync(roleId, Factory);
        var t3 = _cache.GetOrAddAsync(roleId, Factory);

        await Task.Delay(50);
        release.SetResult();

        var results = await Task.WhenAll(t1, t2, t3);

        Assert.That(factoryEntered, Is.EqualTo(1),
            "Per-key lock + double-check should prevent the thundering herd.");
        Assert.That(results, Has.All.EquivalentTo(new[] { "X" }));
    }

    [Test]
    public async Task GetOrAddAsync_DifferentKeys_RunFactoriesInParallel()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var bothEntered = new SemaphoreSlim(0, 2);
        var release = new TaskCompletionSource();

        async Task<IReadOnlyCollection<string>> Factory(CancellationToken _)
        {
            bothEntered.Release();
            await release.Task;
            return new[] { "X" };
        }

        var t1 = _cache.GetOrAddAsync(keyA, Factory);
        var t2 = _cache.GetOrAddAsync(keyB, Factory);

        var firstEntered = await bothEntered.WaitAsync(TimeSpan.FromSeconds(2));
        var secondEntered = await bothEntered.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.That(firstEntered, Is.True);
        Assert.That(secondEntered, Is.True);

        release.SetResult();
        await Task.WhenAll(t1, t2);
    }
}
