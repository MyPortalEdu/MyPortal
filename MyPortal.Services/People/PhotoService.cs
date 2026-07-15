using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.FileStorage.Exceptions;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <inheritdoc cref="IPhotoService"/>
public class PhotoService : BaseService, IPhotoService
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IDocumentService _documentService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PhotoService(IAuthorizationService authorizationService, ILogger<PhotoService> logger,
        IPhotoRepository photoRepository, IPersonRepository personRepository,
        IDocumentService documentService, IUnitOfWorkFactory unitOfWorkFactory)
        : base(authorizationService, logger)
    {
        _photoRepository = photoRepository;
        _personRepository = personRepository;
        _documentService = documentService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task SetPhotoAsync(Guid personId, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(contentType) ||
            !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The uploaded file must be an image.", "file");
        }

        var person = await _personRepository.GetByIdAsync(personId, cancellationToken)
                     ?? throw new NotFoundException("Person not found.");

        ResizedImage resized;
        try
        {
            resized = await ImageResizer.ResizeAsync(image, SystemPhotos.MaxEdgePixels, cancellationToken);
        }
        catch (InvalidImageException ex)
        {
            // Surface as a 400 (see ExceptionMiddleware's ArgumentException handling).
            throw new ArgumentException(ex.Message, "file");
        }

        // The resized image is stored as a normal Document (reusing the whole storage pipeline), but
        // in the dedicated system Photos directory so it never appears in the person's Documents tab.
        DocumentDetailsResponse newDocument;
        try
        {
            var request = new DocumentUpsertRequest
            {
                TypeId = SystemPhotos.PhotographDocumentTypeId,
                DirectoryId = SystemPhotos.DirectoryId,
                Title = string.IsNullOrWhiteSpace(fileName) ? "Photo" : fileName,
                IsPrivate = true,
                FileName = $"{personId:N}{resized.Extension}",
                Content = resized.Stream,
                ContentType = resized.ContentType,
                SizeBytes = resized.Stream.Length
            };

            newDocument = await _documentService.CreateAsync(request, cancellationToken);
        }
        catch
        {
            await resized.DisposeAsync();
            throw;
        }

        var oldPhotoId = person.PhotoId;

        try
        {
            await _unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
            {
                var photo = new Photo
                {
                    Id = SqlConvention.SequentialGuid(),
                    DocumentId = newDocument.Id,
                    PhotoDate = DateTime.UtcNow
                };
                await _photoRepository.InsertAsync(photo, cancellationToken, ownedUow.Transaction);

                person.PhotoId = photo.Id;
                await _personRepository.UpdateAsync(person, cancellationToken, ownedUow.Transaction);

                // Replacing: purge the previous photo within the same transaction (its blob purge is
                // deferred to commit by DocumentService).
                if (oldPhotoId is not null)
                {
                    await PurgePhotoAsync(oldPhotoId.Value, cancellationToken, ownedUow);
                }
            }, cancellationToken);
        }
        catch
        {
            // The document + blob were created outside the transaction, so compensate for the
            // rolled-back Photo/Person changes by removing them.
            try
            {
                await _documentService.DeleteAsync(newDocument.Id, CancellationToken.None, softDelete: false);
            }
            catch (Exception cleanupEx)
            {
                Logger.LogWarning(cleanupEx,
                    "Set-photo failed and the compensating delete of document {documentId} also failed. Manual cleanup may be required.",
                    newDocument.Id);
            }
            throw;
        }
    }

    public async Task<DocumentContentResponse> GetPhotoContentAsync(Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken)
                     ?? throw new NotFoundException("Person not found.");

        if (person.PhotoId is null)
        {
            throw new NotFoundException("This person has no photo.");
        }

        var photo = await _photoRepository.GetByIdAsync(person.PhotoId.Value, cancellationToken)
                    ?? throw new NotFoundException("This person has no photo.");

        return await _documentService.GetDocumentWithContentByIdAsync(photo.DocumentId, cancellationToken);
    }

    public async Task DeletePhotoAsync(Guid personId, CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken)
                     ?? throw new NotFoundException("Person not found.");

        var photoId = person.PhotoId;
        if (photoId is null)
        {
            return;
        }

        await _unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
        {
            // Release the People.PhotoId FK before purging the Photos row.
            person.PhotoId = null;
            await _personRepository.UpdateAsync(person, cancellationToken, ownedUow.Transaction);

            await PurgePhotoAsync(photoId.Value, cancellationToken, ownedUow);
        }, cancellationToken);
    }

    public async Task PurgePhotoAsync(Guid photoId, CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);
        if (photo is null)
        {
            return;
        }

        // Photos.DocumentId FK: drop the Photos row first, then hard-delete the document (which
        // purges the blob).
        await _photoRepository.DeleteAsync(photoId, cancellationToken, softDelete: false, uow?.Transaction);
        await _documentService.DeleteAsync(photo.DocumentId, cancellationToken, softDelete: false, uow);
    }
}
