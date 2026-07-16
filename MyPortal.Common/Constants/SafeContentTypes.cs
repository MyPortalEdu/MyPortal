namespace MyPortal.Common.Constants;

/// <summary>
/// MIME types the server is willing to echo back as <c>Content-Type</c> on
/// document downloads. Any uploaded type outside this set is downgraded to
/// <c>application/octet-stream</c> on download so browsers force a save instead
/// of attempting to render — defeating stored-XSS via attacker-controlled
/// content type. Notably excludes: text/html, image/svg+xml, application/xml,
/// any text/javascript variants, and anything else a browser will execute.
/// </summary>
public static class SafeContentTypes
{
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        // Images — SVG deliberately excluded (can carry inline JavaScript)
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp",
        "image/tiff",

        "application/pdf",
        "text/plain",
        "text/csv",

        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",

        "application/zip",
        "application/x-zip-compressed"
    };

    public const string Fallback = "application/octet-stream";

    public static string Sanitize(string? contentType) =>
        !string.IsNullOrWhiteSpace(contentType) && All.Contains(contentType)
            ? contentType
            : Fallback;
}
