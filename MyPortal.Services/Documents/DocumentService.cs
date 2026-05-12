using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.FileStorage.Helpers;
using MyPortal.FileStorage.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Documents;

namespace MyPortal.Services.Documents;

/// <inheritdoc cref="IDocumentService"/>
public class DocumentService : BaseService, IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentTypeRepository _documentTypeRepository;
    private readonly IStorageKeyGenerator _storageKeyGenerator;
    private readonly IFileStorageProvider _storageProvider;
    private readonly IValidationService _validationService;
    private readonly IOptions<FileStorageOptions> _fileStorageOptions;

    public DocumentService(IAuthorizationService authorizationService, ILogger<DocumentService> logger,
        IDocumentRepository documentRepository, IDocumentTypeRepository documentTypeRepository,
        IStorageKeyGenerator storageKeyGenerator, IFileStorageProvider storageProvider,
        IValidationService validationService, IOptions<FileStorageOptions> fileStorageOptions)
        : base(authorizationService, logger)
    {
        _documentRepository = documentRepository;
        _documentTypeRepository = documentTypeRepository;
        _storageKeyGenerator = storageKeyGenerator;
        _storageProvider = storageProvider;
        _validationService = validationService;
        _fileStorageOptions = fileStorageOptions;
    }

    private long MaxFileSizeBytes => _fileStorageOptions.Value.MaxFileSizeBytes;

    public async Task<DocumentDetailsResponse> CreateAsync(DocumentUpsertRequest model,
        CancellationToken cancellationToken)
    {
        if (model.Content == null || model.SizeBytes <= 0 || !model.Content.CanRead)
        {
            throw new ArgumentException("Document has no content.", nameof(model.Content));
        }

        await _validationService.ValidateAsync(model);

        if (model.IsPrivate && AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException("You do not have permission to create private documents.");
        }

        var storageKey = _storageKeyGenerator.Generate(model.FileName!);

        await using var hashedStream =
            await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, MaxFileSizeBytes, cancellationToken);

        // Trust the stream over the client-supplied SizeBytes. Reject mismatches — a client
        // that claims SizeBytes=100 while uploading something larger or smaller is either
        // malicious or buggy, and we don't want the DB row to disagree with the blob.
        VerifyContentLength(hashedStream.UsableStream.Length, model.SizeBytes);

        await _storageProvider.SaveFileAsync(storageKey, hashedStream.UsableStream, model.ContentType!,
            cancellationToken);

        Logger.LogInformation("File {fileName} saved with storage key: {storageKey}", model.FileName, storageKey);

        var id = SqlConvention.SequentialGuid();

        var document = new Document
        {
            Id = id,
            StorageKey = storageKey,
            ContentType = model.ContentType!,
            FileName = model.FileName!,
            DirectoryId = model.DirectoryId,
            SizeBytes = hashedStream.UsableStream.Length,
            TypeId = model.TypeId,
            Title = model.Title,
            Description = model.Description,
            Hash = hashedStream.Hash,
            IsPrivate = model.IsPrivate
        };

        await _documentRepository.InsertAsync(document, cancellationToken);

        Logger.LogInformation("Document created: {documentId}", id);

        var response = await _documentRepository.GetDetailsByIdAsync(id, cancellationToken)
                       ?? throw new InvalidOperationException("Created document, but could not load details.");

        return response;
    }

    public async Task<DocumentDetailsResponse> UpdateAsync(Guid documentId, DocumentUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _validationService.ValidateAsync(model);
            
        var documentInDb = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (documentInDb == null)
        {
            throw new NotFoundException("Document not found.");
        }

        var isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff;

        if (!isStaff)
        {
            // Ownership: non-staff can only edit their own documents
            if (documentInDb.CreatedById != AuthorizationService.GetCurrentUserId())
                throw new ForbiddenException("You do not have permission to modify this document.");

            // Strong rule: cannot touch private docs at all
            if (documentInDb.IsPrivate)
                throw new ForbiddenException("You do not have permission to modify this document.");

            // Cannot make private
            if (model.IsPrivate)
                throw new ForbiddenException("You do not have permission to make documents private.");
        }

        documentInDb.TypeId = model.TypeId;
        documentInDb.DirectoryId = model.DirectoryId;
        documentInDb.Title = model.Title;
        documentInDb.Description = model.Description;
        documentInDb.IsPrivate = model.IsPrivate;

        string? oldStorageKey = null;
        string? newStorageKey = null;

        if (model.Content != null)
        {
            newStorageKey = _storageKeyGenerator.Generate(model.FileName!);

            await using var hashedStream =
                await FileStorageHasher.HashAndPrepareStreamAsync(model.Content, MaxFileSizeBytes, cancellationToken);

            VerifyContentLength(hashedStream.UsableStream.Length, model.SizeBytes);

            await _storageProvider.SaveFileAsync(newStorageKey, hashedStream.UsableStream,
                model.ContentType!, cancellationToken);

            Logger.LogInformation("New file {fileName} saved with storage key: {storageKey}", model.FileName, newStorageKey);

            oldStorageKey = documentInDb.StorageKey;
            documentInDb.StorageKey = newStorageKey;
            documentInDb.FileName = model.FileName!;
            documentInDb.ContentType = model.ContentType!;
            documentInDb.SizeBytes = hashedStream.UsableStream.Length;
            documentInDb.Hash = hashedStream.Hash;
        }

        try
        {
            await _documentRepository.UpdateAsync(documentInDb, cancellationToken);
        }
        catch
        {
            if (newStorageKey != null)
            {
                try
                {
                    await _storageProvider.DeleteFileAsync(newStorageKey, CancellationToken.None);
                }
                catch (Exception cleanupEx)
                {
                    Logger.LogWarning(cleanupEx,
                        "Document update failed; orphan file left at storage key {storageKey} (cleanup also failed).",
                        newStorageKey);
                }
            }
            throw;
        }

        Logger.LogInformation("Document updated: {documentId}", documentId);

        if (oldStorageKey != null)
        {
            try
            {
                await _storageProvider.DeleteFileAsync(oldStorageKey, cancellationToken);
                Logger.LogInformation("Replaced file deleted at storage key: {storageKey}", oldStorageKey);
            }
            catch (Exception cleanupEx)
            {
                Logger.LogWarning(cleanupEx,
                    "Document updated, but failed to delete replaced file at storage key {storageKey}. Manual cleanup may be required.",
                    oldStorageKey);
            }
        }

        var response = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken)
                       ?? throw new InvalidOperationException("Updated document, but could not load details.");

        return response;
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken cancellationToken, bool softDelete = true,
        IUnitOfWork? uow = null)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);

        if (document == null)
        {
            throw new NotFoundException("Document not found.");
        }

        if (document.IsPrivate && AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException("You do not have permission to delete this document.");
        }

        // Delete the DB row first; only purge the blob on success. Reverse order would orphan
        // the row pointing at a missing blob if storage delete succeeded but DB delete failed.
        await _documentRepository.DeleteAsync(documentId, cancellationToken, softDelete, uow?.Transaction);

        Logger.LogInformation("Document deleted: {documentId}, soft delete: {softDelete}", documentId, softDelete);

        if (!softDelete)
        {
            try
            {
                await _storageProvider.DeleteFileAsync(document.StorageKey, cancellationToken);
                Logger.LogInformation("File {fileName} deleted, storage key: {storageKey}", document.FileName,
                    document.StorageKey);
            }
            catch (Exception cleanupEx)
            {
                Logger.LogWarning(cleanupEx,
                    "Document row deleted but failed to delete blob at storage key {storageKey}. Manual cleanup may be required.",
                    document.StorageKey);
            }
        }
    }

    public async Task<DocumentDetailsResponse> GetDocumentByIdAsync(Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);

        if (result == null)
        {
            throw new NotFoundException("Document not found.");
        }

        return result;
    }

    public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId,
        CancellationToken cancellationToken)
    {
        var documentDetails = await _documentRepository.GetDetailsByIdAsync(documentId, cancellationToken);

        if (documentDetails == null)
        {
            throw new NotFoundException("Document not found.");
        }

        if (documentDetails.IsPrivate && AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            throw new ForbiddenException("You do not have permission to view this document.");
        }

        // The caller is responsible for disposing the returned stream
        // We use try-catch to ensure proper disposal if an exception occurs after opening the stream
        Stream? content = null;
        try
        {
            content = await _storageProvider.OpenReadFileAsync(documentDetails.StorageKey, cancellationToken);
                
            var response = new DocumentContentResponse
            {
                Details = documentDetails,
                Content = content
            };

            return response;
        }
        catch (Exception)
        {
            if (content != null)
            {
                await content.DisposeAsync();
            }
                
            throw;
        }
    }

    public async Task<IList<LookupResponse>> GetDocumentTypesAsync(DocumentTypeFilter filter,
        CancellationToken cancellationToken)
    {
        var all = await _documentTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return all
            .Where(t => t.Active)
            .Where(t => (filter.General && t.General) || (filter.Student && t.Student) ||
                        (filter.Contact && t.Contact) || (filter.Send && t.IsSend) ||
                        (filter.Staff && t.Staff))
            .Select(t => t.ToResponseModel())
            .ToList();
    }

    public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsByDirectoryId(Guid directoryId,
        CancellationToken cancellationToken, bool includeDeleted = false)
    {
        return await _documentRepository.GetDocumentsByDirectoryId(directoryId, cancellationToken, includeDeleted);
    }

    public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsInSubtreeAsync(Guid directoryId,
        CancellationToken cancellationToken, bool includeDeleted = false)
    {
        return await _documentRepository.GetDocumentsInSubtreeAsync(directoryId, cancellationToken, includeDeleted);
    }

    private static void VerifyContentLength(long actual, long? claimed)
    {
        if (claimed.HasValue && claimed.Value != actual)
        {
            throw new ArgumentException(
                $"Uploaded content length ({actual} bytes) does not match the declared size ({claimed.Value} bytes).",
                nameof(DocumentUpsertRequest.SizeBytes));
        }
    }
}