using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Interfaces;

namespace MyPortal.Services.Interfaces.Services;

/// <summary>
/// Defines operations for managing directories and documents within an entity's directory structure, including
/// creation, retrieval, update, deletion, and content access.
/// </summary>
/// <remarks>This interface provides asynchronous methods for handling directories and documents associated with a
/// specific entity, identified by a unique ID. Implementations are expected to support operations such as creating and
/// updating directories and documents, retrieving details and contents, and deleting items with options for soft
/// deletion. All methods accept a cancellation token to support cooperative cancellation of asynchronous operations.
/// Thread safety and transactional guarantees depend on the specific implementation.</remarks>
public interface IDirectoryEntityService<TDirectoryEntity> where TDirectoryEntity : IDirectoryEntity
{
    /// <summary>
    /// Creates a new directory for the specified entity asynchronously using the provided details.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity for which the directory will be created.</param>
    /// <param name="model">An object containing the properties and configuration for the new directory. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a DirectoryDetailsResponse with
    /// information about the newly created directory.</returns>
    Task<DirectoryDetailsResponse> CreateDirectoryAsync(Guid entityId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates the details of an existing directory for the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity that owns the directory to be updated.</param>
    /// <param name="directoryId">The unique identifier of the directory to update.</param>
    /// <param name="model">An object containing the updated directory information to apply.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a response with the updated
    /// directory details.</returns>
    Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid entityId, Guid directoryId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes the specified directory associated with the given entity.
    /// </summary>
    /// <remarks>If the directory contains files or subdirectories, they will also be deleted. The operation
    /// is idempotent; attempting to delete a non-existent directory will not result in an error.</remarks>
    /// <param name="entityId">The unique identifier of the entity that owns the directory to be deleted.</param>
    /// <param name="directoryId">The unique identifier of the directory to delete.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the details of a directory identified by the specified directory ID within the given
    /// entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity that contains the directory. This value must not be empty.</param>
    /// <param name="directoryId">The unique identifier of the directory to retrieve. This value must not be empty.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="DirectoryDetailsResponse"/> with the directory details if found; otherwise, <see langword="null"/>.</returns>
    Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the contents of a specified directory associated with the given entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity whose directory contents are to be retrieved.</param>
    /// <param name="directoryId">The unique identifier of the directory within the entity to retrieve contents for.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a DirectoryContentsResponse with the
    /// details of the directory's contents.</returns>
    Task<DirectoryContentsResponse>
        GetDirectoryContentsAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves the hierarchical structure of a directory and its subdirectories for the specified
    /// entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity whose directory tree is to be retrieved.</param>
    /// <param name="directoryId">The unique identifier of the root directory from which to begin building the tree.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <param name="includeDeletedDocs">Specifies whether to include documents that have been marked as deleted in the directory tree. Set to <see
    /// langword="true"/> to include deleted documents; otherwise, <see langword="false"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="DirectoryTreeResponse"/> object representing the directory tree structure.</returns>
    Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken, bool includeDeletedDocs = true);

    /// <summary>
    /// Creates a new document for the specified entity using the provided details.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to which the document will be associated.</param>
    /// <param name="model">The details of the document to create, including metadata and content. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a response with details of the
    /// created document.</returns>
    Task<DocumentDetailsResponse> CreateDocumentAsync(Guid entityId, DocumentUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates an existing document for the specified entity using the provided data.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to which the document belongs.</param>
    /// <param name="documentId">The unique identifier of the document to update.</param>
    /// <param name="model">The data used to update the document. Must not be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a response with details of the
    /// updated document.</returns>
    Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid entityId, Guid documentId, DocumentUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously deletes a document associated with the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to which the document belongs.</param>
    /// <param name="documentId">The unique identifier of the document to delete.</param>
    /// <param name="cancellationToken">A token that can be used to request cancellation of the delete operation.</param>
    /// <param name="softDelete">If <see langword="true"/>, performs a soft delete by marking the document as deleted without removing it from
    /// storage; otherwise, permanently deletes the document.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteDocumentAsync(Guid entityId, Guid documentId, CancellationToken cancellationToken,
        bool softDelete = true);

    /// <summary>
    /// Asynchronously retrieves the details of a document associated with the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to which the document belongs.</param>
    /// <param name="documentId">The unique identifier of the document to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="DocumentDetailsResponse"/> with the document details if found; otherwise, <see langword="null"/>.</returns>
    Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously retrieves a document and its associated content by document identifier for the specified entity.
    /// </summary>
    /// <param name="entityId">The unique identifier of the entity to which the document belongs.</param>
    /// <param name="documentId">The unique identifier of the document to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="DocumentContentResponse"/> with the document and its content if found; otherwise, the result may indicate
    /// that the document does not exist.</returns>
    Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken);
}