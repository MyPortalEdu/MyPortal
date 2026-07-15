namespace MyPortal.Common.Constants;

/// <summary>
/// Seeded by <c>20260707000000_person_photo_support.sql</c>; the directory is flagged system by
/// <c>20260715000000_directory_is_system.sql</c>. Id-keyed of necessity — <c>Directories</c> has no
/// discriminator and <c>DocumentTypes</c> has only facet flags, so there is nothing else stable to
/// match on.
/// </summary>
public static class SystemPhotos
{
    public static readonly Guid DirectoryId = new("0F0705D1-0000-4000-8000-000000000001");

    public static readonly Guid PhotographDocumentTypeId = new("5DD555DE-0C38-4FCC-BB54-C3C4A7E81201");

    public const int MaxEdgePixels = 512;
}
