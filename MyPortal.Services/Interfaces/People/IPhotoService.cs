using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Person-level photo management, reusable across person subtypes (staff, and later students /
/// parents — they share <c>People.PhotoId</c>). Performs NO authorization of its own; callers (e.g.
/// the staff service) apply the relevant subtype gate before delegating here.
///
/// <para>
/// A photo is stored as a <c>Document</c> in a dedicated system directory (outside any person's
/// browsable attachments subtree), wrapped by a <c>Photos</c> row referenced by
/// <c>Person.PhotoId</c>.
/// </para>
/// </summary>
public interface IPhotoService
{
    /// <summary>
    /// Resizes and stores <paramref name="image"/> as the person's photo, replacing (and purging) any
    /// existing one.
    /// </summary>
    /// <exception cref="System.ArgumentException">The upload is not a decodable image.</exception>
    Task SetPhotoAsync(Guid personId, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Opens the person's photo for streaming. The caller owns and must dispose
    /// <see cref="DocumentContentResponse.Content"/>.
    /// </summary>
    /// <exception cref="MyPortal.Common.Exceptions.NotFoundException">The person has no photo.</exception>
    Task<DocumentContentResponse> GetPhotoContentAsync(Guid personId, CancellationToken cancellationToken);

    /// <summary>Removes the person's photo (nulls <c>Person.PhotoId</c> and purges the document + blob). No-op if none.</summary>
    Task DeletePhotoAsync(Guid personId, CancellationToken cancellationToken);

    /// <summary>
    /// Purges a photo's row + document + blob without touching any <c>Person</c>. Used by the
    /// person-delete cascade, which has already removed the referencing person. No-op if the photo
    /// no longer exists.
    /// </summary>
    Task PurgePhotoAsync(Guid photoId, CancellationToken cancellationToken, IUnitOfWork? uow = null);
}
