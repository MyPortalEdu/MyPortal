namespace MyPortal.Common.Constants;

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
