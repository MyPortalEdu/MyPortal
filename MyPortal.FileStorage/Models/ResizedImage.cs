namespace MyPortal.FileStorage.Models;

/// <summary>
/// A normalised, resized raster image ready to be stored. <see cref="Stream"/> is a fresh, seekable
/// stream positioned at 0 that the caller owns and must dispose.
/// </summary>
public sealed class ResizedImage : IDisposable, IAsyncDisposable
{
    public required Stream Stream { get; init; }
    public required string ContentType { get; init; }

    /// <summary>File extension (with leading dot, e.g. ".jpg") — the storage-key generator requires one.</summary>
    public required string Extension { get; init; }

    public void Dispose() => Stream.Dispose();

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
