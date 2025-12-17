using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Filters;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Interfaces.Services
{
    /// <summary>
    /// Defines operations for creating, updating, deleting, and retrieving documents and document types within the
    /// system.
    /// </summary>
    /// <remarks>Implementations of this interface should ensure thread safety if accessed concurrently.
    /// Methods support cancellation via <see cref="CancellationToken"/> to allow responsive and robust client
    /// applications. Returned streams from <see cref="GetDocumentWithContentByIdAsync"/> must be disposed of by the
    /// caller to avoid resource leaks.</remarks>
    public interface IDocumentService
    {
        /// <summary>
        /// Creates a new document asynchronously using the specified request model.
        /// </summary>
        /// <param name="model">The request containing the document data to create or update. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the details of the created or
        /// updated document.</returns>
        Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously updates the specified document with new data and returns the updated document details.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to update.</param>
        /// <param name="model">An object containing the updated values for the document. Cannot be null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. Passing a canceled token will attempt to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the details of the updated
        /// document.</returns>
        Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously deletes the specified document by its unique identifier. Supports both soft and hard deletion
        /// based on the provided option.
        /// </summary>
        /// <remarks>If soft deletion is enabled, the document remains in storage and may be recoverable
        /// depending on system capabilities. Hard deletion permanently removes the document and its data. The operation
        /// is idempotent; attempting to delete a non-existent document has no effect.</remarks>
        /// <param name="documentId">The unique identifier of the document to delete.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the delete operation.</param>
        /// <param name="softDelete">If <see langword="true"/>, performs a soft delete by marking the document as deleted without removing it
        /// from storage; if <see langword="false"/>, permanently removes the document.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken, bool softDelete = true);

        /// <summary>
        /// Asynchronously retrieves the details of a document by its unique identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DocumentDetailsResponse"/> with the document details if found; otherwise, <see langword="null"/>.</returns>
        Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously retrieves a document and its associated content by the specified document identifier.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to retrieve.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DocumentContentResponse"/> with the document and its content if found; otherwise, the result may
        /// indicate that the document does not exist.</returns>
        Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId,
            CancellationToken cancellationToken);
        
        /// <summary>
        /// Asynchronously retrieves a list of document types that match the specified filter criteria.
        /// </summary>
        /// <param name="filter">An object containing the criteria used to filter the document types. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of document types
        /// matching the filter. The list will be empty if no document types are found.</returns>
        Task<IList<LookupResponse>> GetDocumentTypesAsync(DocumentTypeFilter filter, CancellationToken cancellationToken);
    }
}
