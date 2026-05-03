using System.Threading.Channels;

namespace MyPortal.Services.Timetable;

public record TimetableRunWorkItem(Guid RunId, Guid TimetableId, Guid WeekPatternId);

/// In-process FIFO queue of solver runs waiting to execute. Single host, ephemeral —
/// items not yet picked up by the worker are lost on host restart, and the worker's
/// startup sweep marks any orphaned Run rows as Failed so the polling endpoint shows the
/// right thing rather than spinning forever.
public class TimetableRunQueue
{
    private readonly Channel<TimetableRunWorkItem> _channel = Channel.CreateUnbounded<TimetableRunWorkItem>(
        new UnboundedChannelOptions
        {
            // Single producer per HTTP request, single consumer (the BackgroundService).
            SingleReader = true,
            SingleWriter = false,
        });

    public ValueTask EnqueueAsync(TimetableRunWorkItem item, CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(item, cancellationToken);

    public IAsyncEnumerable<TimetableRunWorkItem> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
