namespace MyPortal.FileStorage.Models;

public class HashedStream : IDisposable, IAsyncDisposable
{
    public string Hash { get; set; } = null!;
    public Stream UsableStream { get; set; } = null!;

    public void Dispose()
    {
        UsableStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await UsableStream.DisposeAsync();
    }
}