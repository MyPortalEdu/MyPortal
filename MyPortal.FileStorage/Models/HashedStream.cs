namespace MyPortal.FileStorage.Models;

public class HashedStream : IDisposable, IAsyncDisposable
{
    public required string Hash { get; set; }
    public required Stream UsableStream { get; set; }

    public void Dispose()
    {
        UsableStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await UsableStream.DisposeAsync();
    }
}