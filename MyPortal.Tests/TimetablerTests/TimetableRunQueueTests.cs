using MyPortal.Services.Timetable;

namespace MyPortal.Tests.TimetablerTests;

[TestFixture]
public class TimetableRunQueueTests
{
    private TimetableRunQueue _queue = null!;

    [SetUp]
    public void Setup() => _queue = new TimetableRunQueue();

    private static TimetableRunWorkItem Item() =>
        new(Guid.NewGuid(), Guid.NewGuid());

    [Test]
    public async Task Enqueue_And_Read_RoundtripsItem()
    {
        var item = Item();
        await _queue.EnqueueAsync(item, CancellationToken.None);

        // Take the first item from the async enumerable then break out — the channel never
        // completes on its own (it's unbounded and never closed by tests), so we cap reads
        // by passing a CT we cancel after the first hit.
        using var cts = new CancellationTokenSource();
        await foreach (var read in _queue.ReadAllAsync(cts.Token))
        {
            Assert.That(read, Is.EqualTo(item));
            cts.Cancel();
            break;
        }
    }

    [Test]
    public async Task Reads_AreFifoOrdered()
    {
        var a = Item();
        var b = Item();
        var c = Item();
        await _queue.EnqueueAsync(a, CancellationToken.None);
        await _queue.EnqueueAsync(b, CancellationToken.None);
        await _queue.EnqueueAsync(c, CancellationToken.None);

        var read = new List<TimetableRunWorkItem>();
        using var cts = new CancellationTokenSource();
        await foreach (var item in _queue.ReadAllAsync(cts.Token))
        {
            read.Add(item);
            if (read.Count == 3)
            {
                cts.Cancel();
                break;
            }
        }

        Assert.That(read, Is.EqualTo(new[] { a, b, c }));
    }

    [Test]
    public async Task Read_BlocksUntilEnqueue_ThenReturnsItem()
    {
        // Start reading on a background task before any item is enqueued. The reader should
        // be parked on the channel; once we enqueue, it should wake up with our item.
        using var cts = new CancellationTokenSource();
        var item = Item();

        var readTask = Task.Run(async () =>
        {
            await foreach (var x in _queue.ReadAllAsync(cts.Token))
            {
                cts.Cancel();
                return x;
            }
            throw new InvalidOperationException("reader exited without producing an item");
        });

        // Tiny grace period to ensure the reader is parked, not just slow.
        await Task.Delay(20);
        Assert.That(readTask.IsCompleted, Is.False, "Reader should not complete before enqueue.");

        await _queue.EnqueueAsync(item, CancellationToken.None);
        var actual = await readTask;

        Assert.That(actual, Is.EqualTo(item));
    }
}
