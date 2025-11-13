using System.Security.Cryptography;
using MyPortal.FileStorage.Models;

namespace MyPortal.FileStorage.Helpers;

public static class FileStorageHasher
{
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