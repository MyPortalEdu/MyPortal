using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Services;

public interface IDirectoryEntityService
{
    Task<DirectoryDetailsResponse> CreateDirectoryAsync(Guid entityId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken);

    Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid entityId, Guid directoryId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken);

    Task DeleteDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken);

    Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken);

    Task<DirectoryContentsResponse>
        GetDirectoryContentsAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken);

    Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken, bool includeDeletedDocs = true);

    Task<DocumentDetailsResponse> CreateDocumentAsync(Guid entityId, DocumentUpsertRequest model, CancellationToken cancellationToken);

    Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid entityId, Guid documentId, DocumentUpsertRequest model,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(Guid entityId, Guid documentId, CancellationToken cancellationToken,
        bool softDelete = true);

    Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken);
    
    Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken);
}