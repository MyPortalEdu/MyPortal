using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Interfaces.Services
{
    /// <summary>
    /// Service used to manage <see cref="Document"/> entities.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Creates a new document from the provided upsert model.
        /// </summary>
        /// <param name="model">The document data used to create the document.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="DocumentDetailsResponse"/> containing the created document's details.
        /// </returns>
        Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing document identified by <paramref name="documentId"/> with the provided model.
        /// </summary>
        /// <param name="documentId">The identifier of the document to update.</param>
        /// <param name="model">The document data to apply to the existing document.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="DocumentDetailsResponse"/> containing the updated document's details.
        /// </returns>
        Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the document with the specified identifier.
        /// </summary>
        /// <param name="documentId">The identifier of the document to delete.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> that completes when the deletion is finished.</returns>
        Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves detailed information for a document by its identifier.
        /// </summary>
        /// <param name="documentId">The identifier of the document to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        /// A <see cref="DocumentDetailsResponse"/> with the document details if found; otherwise <c>null</c>.
        /// </returns>
        Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a document including its contents by its identifier.
        /// </summary>
        /// <param name="documentId">The identifier of the document to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A <see cref="DocumentContentResponse"/> with the document details and its contents.</returns>
        /// <remarks>The stream returned with the <see cref="DocumentContentResponse"/> must be disposed of by the caller.</remarks>
        Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId, CancellationToken cancellationToken);
    }
}
