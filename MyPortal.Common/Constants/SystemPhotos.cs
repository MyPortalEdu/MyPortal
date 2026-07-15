namespace MyPortal.Common.Constants;

/// <summary>
/// Well-known ids for the person-photo storage lane, seeded by
/// <c>20260707000000_person_photo_support.sql</c>.
///
/// <para>
/// A photo is stored as a <c>Document</c> (via the file-storage pipeline) referenced by a
/// <c>Photos</c> row referenced by <c>Person.PhotoId</c>. The photo's document lives in a dedicated
/// <b>system</b> directory that no per-entity attachments browser is ever scoped to — so it never
/// appears in a person's Documents tab and can't be deleted through that UI.
/// </para>
/// </summary>
public static class SystemPhotos
{
    /// <summary>The system directory that holds every person-photo document.</summary>
    public static readonly Guid DirectoryId =
        new("0F0705D1-0000-4000-8000-000000000001");

    /// <summary>The "Photograph" document type (IsSystem, no facet flags → hidden from the upload picker).</summary>
    public static readonly Guid PhotographDocumentTypeId =
        new("5DD555DE-0C38-4FCC-BB54-C3C4A7E81201");

    /// <summary>Longest-edge cap (px) applied to uploaded photos.</summary>
    public const int MaxEdgePixels = 512;
}
