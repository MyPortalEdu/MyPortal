using System.Security.Cryptography;
using MyPortal.FileStorage.Models;

namespace MyPortal.FileStorage.Helpers;

/// <summary>
/// Provides functionality to compute SHA-256 hashes for file streams used in file storage.
/// </summary>
public static class FileStorageHasher
{
    /// <summary>
    /// Computes the SHA-256 hash of the provided stream and returns a new stream positioned at the beginning, along
    /// with the computed hash.
    /// </summary>
    /// <remarks>If the input stream is seekable, the original stream is reused and its position is reset to
    /// zero before and after hashing. If the stream is not seekable, the contents are buffered into memory. The caller
    /// is responsible for disposing the returned stream when it is no longer needed.</remarks>
    /// <param name="input">The input stream to hash. The stream must be readable. If the stream supports seeking, it will be reset to the
    /// beginning before and after hashing.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a HashedStream object with the
    /// computed hash and a stream positioned at the beginning for further reading.</returns>
    public static async Task<HashedStream> HashAndPrepareStreamAsync(
        Stream input,
        CancellationToken ct = default)
    {
        if (input.CanSeek)
        {
            // Fast path: reuse the original stream
            input.Position = 0;

            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(input, ct);
            var hashHex = Convert.ToHexString(hashBytes);

            input.Position = 0;

            return new HashedStream
            {
                Hash = hashHex,
                UsableStream = input
            };
        }

        // Slow path: non-seekable stream, buffer once
        var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using (var sha256 = SHA256.Create())
        {
            var hashBytes = await sha256.ComputeHashAsync(buffer, ct);
            var hashHex = Convert.ToHexString(hashBytes);

            buffer.Position = 0;

            return new HashedStream
            {
                Hash = hashHex,
                UsableStream = buffer
            };
        }
    }
}