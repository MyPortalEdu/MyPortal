using MyPortal.FileStorage.Exceptions;
using MyPortal.FileStorage.Models;
using SkiaSharp;

namespace MyPortal.FileStorage.Helpers;

/// <summary>
/// Decodes an uploaded raster image, normalises EXIF orientation, downscales it to fit within a
/// square bound (preserving aspect ratio, never upscaling) and re-encodes it to JPEG. Used to
/// produce a modest, uniform person photo from an arbitrary upload.
/// </summary>
public static class ImageResizer
{
    private const int JpegQuality = 85;

    /// <summary>
    /// Produces a resized JPEG from <paramref name="input"/>. The returned <see cref="ResizedImage"/>
    /// owns a fresh seekable stream at position 0.
    /// </summary>
    /// <param name="input">The raw upload stream (may be forward-only).</param>
    /// <param name="maxEdge">Maximum length, in pixels, of the longest edge of the output.</param>
    /// <exception cref="InvalidImageException">The input is not a decodable image.</exception>
    public static async Task<ResizedImage> ResizeAsync(Stream input, int maxEdge, CancellationToken ct = default)
    {
        if (maxEdge <= 0) throw new ArgumentOutOfRangeException(nameof(maxEdge));

        // SKCodec needs to seek (headers then pixels); the upload stream may be forward-only, so
        // buffer it first.
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using var codec = SKCodec.Create(buffer)
            ?? throw new InvalidImageException("The uploaded file is not a readable image.");

        var decoded = SKBitmap.Decode(codec)
            ?? throw new InvalidImageException("The uploaded image could not be decoded.");
        try
        {
            // Decode ignores EXIF orientation — apply it so rotated phone photos display upright.
            var oriented = ApplyOrientation(decoded, codec.EncodedOrigin);
            try
            {
                var (targetW, targetH) = ScaledSize(oriented.Width, oriented.Height, maxEdge);

                SKBitmap? scaled = null;
                try
                {
                    var toEncode = oriented;
                    if (targetW != oriented.Width || targetH != oriented.Height)
                    {
                        scaled = oriented.Resize(new SKImageInfo(targetW, targetH), SKFilterQuality.High)
                            ?? throw new InvalidImageException("The uploaded image could not be resized.");
                        toEncode = scaled;
                    }

                    using var image = SKImage.FromBitmap(toEncode);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality)
                        ?? throw new InvalidImageException("The uploaded image could not be encoded.");

                    var output = new MemoryStream(data.ToArray()); // seekable, positioned at 0
                    return new ResizedImage
                    {
                        Stream = output,
                        ContentType = "image/jpeg",
                        Extension = ".jpg",
                    };
                }
                finally
                {
                    scaled?.Dispose();
                }
            }
            finally
            {
                if (!ReferenceEquals(oriented, decoded)) oriented.Dispose();
            }
        }
        finally
        {
            decoded.Dispose();
        }
    }

    // Longest-edge cap, aspect-preserving, never upscaling.
    private static (int w, int h) ScaledSize(int w, int h, int maxEdge)
    {
        var longest = Math.Max(w, h);
        if (longest <= maxEdge) return (w, h);
        var scale = (double)maxEdge / longest;
        return (Math.Max(1, (int)Math.Round(w * scale)), Math.Max(1, (int)Math.Round(h * scale)));
    }

    // Normalises the three rotation origins produced by cameras (90/180/270). The rare mirrored
    // origins (2/4/5/7) are left as decoded — cameras effectively never emit them.
    private static SKBitmap ApplyOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        switch (origin)
        {
            case SKEncodedOrigin.BottomRight: // 180°
            {
                var rotated = new SKBitmap(bitmap.Width, bitmap.Height);
                using var canvas = new SKCanvas(rotated);
                canvas.RotateDegrees(180, bitmap.Width / 2f, bitmap.Height / 2f);
                canvas.DrawBitmap(bitmap, 0, 0);
                return rotated;
            }
            case SKEncodedOrigin.RightTop: // 90° CW — swap dimensions
            {
                var rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using var canvas = new SKCanvas(rotated);
                canvas.Translate(rotated.Width, 0);
                canvas.RotateDegrees(90);
                canvas.DrawBitmap(bitmap, 0, 0);
                return rotated;
            }
            case SKEncodedOrigin.LeftBottom: // 270° CW — swap dimensions
            {
                var rotated = new SKBitmap(bitmap.Height, bitmap.Width);
                using var canvas = new SKCanvas(rotated);
                canvas.Translate(0, rotated.Height);
                canvas.RotateDegrees(270);
                canvas.DrawBitmap(bitmap, 0, 0);
                return rotated;
            }
            default:
                return bitmap;
        }
    }
}
