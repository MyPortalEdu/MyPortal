namespace MyPortal.FileStorage.Exceptions;

/// <summary>
/// Thrown when an uploaded file cannot be decoded/processed as a raster image (not an image, or a
/// corrupt/unsupported one). Callers in the service layer translate this to a 400-style response.
/// </summary>
public sealed class InvalidImageException : Exception
{
    public InvalidImageException(string message) : base(message) { }
}
