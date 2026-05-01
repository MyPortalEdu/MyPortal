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
    /// with the computed hash. Enforces an upper bound on the number of bytes consumed so a non-seekable stream
    /// cannot OOM the process if a caller lies about its size.
    /// </summary>
    /// <param name="input">The input stream to hash. Must be readable.</param>
    /// <param name="maxBytes">Maximum allowed byte count. Exceeding this throws <see cref="InvalidOperationException"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="HashedStream"/> with the computed hash and a stream ready to be consumed from offset 0.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the stream contains more than <paramref name="maxBytes"/> bytes.</exception>
    public static async Task<HashedStream> HashAndPrepareStreamAsync(
        Stream input,
        long maxBytes,
        CancellationToken ct = default)
    {
        if (maxBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        if (input.CanSeek)
        {
            // Fast path: reject upfront if Length already exceeds the limit, then reuse the
            // original stream.
            if (input.Length > maxBytes)
                throw new InvalidOperationException($"Upload exceeds maximum size of {maxBytes} bytes.");

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

        // Slow path: non-seekable stream — copy into a MemoryStream while enforcing the cap
        // so a malicious upload doesn't OOM the process.
        var buffer = new MemoryStream();
        var rented = new byte[81920];
        long total = 0;
        int read;

        while ((read = await input.ReadAsync(rented.AsMemory(), ct)) > 0)
        {
            total += read;
            if (total > maxBytes)
                throw new InvalidOperationException($"Upload exceeds maximum size of {maxBytes} bytes.");
            await buffer.WriteAsync(rented.AsMemory(0, read), ct);
        }

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