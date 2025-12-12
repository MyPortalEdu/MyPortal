using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Services
{
    /// <summary>
    /// Defines operations for managing directories and their contents within a hierarchical directory system.
    /// </summary>
    /// <remarks>This interface provides asynchronous methods for creating, updating, deleting, and retrieving
    /// directories, as well as accessing their contents and structure. Implementations are expected to support
    /// cancellation via the provided <see cref="CancellationToken"/> parameters. Methods may return null or empty
    /// results if the specified directory does not exist or contains no items. Thread safety and specific error
    /// handling depend on the concrete implementation.</remarks>
    public interface IDirectoryService
    {
        /// <summary>
        /// Creates a new directory asynchronously based on the specified request model.
        /// </summary>
        /// <param name="model">An object containing the details of the directory to create or update. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with details of the
        /// created or updated directory.</returns>
        Task<DirectoryDetailsResponse> CreateDirectoryAsync(DirectoryUpsertRequest model,
            CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously updates the details of an existing directory with the specified information.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory to update.</param>
        /// <param name="model">An object containing the updated directory information to apply.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a response with the updated
        /// directory details.</returns>
        Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid directoryId, DirectoryUpsertRequest model,
            CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously deletes the directory identified by the specified unique identifier.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteDirectoryAsync(Guid directoryId, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously retrieves the details of a directory identified by the specified unique identifier.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory to retrieve. Must correspond to an existing directory.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DirectoryDetailsResponse"/> with the directory details if found; otherwise, <see langword="null"/>.</returns>
        Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid directoryId, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously retrieves the immediate contents of the specified directory.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory whose contents are to be retrieved.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DirectoryContentsResponse"/> with information about the directory's contents.</returns>
        Task<DirectoryContentsResponse>
            GetDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken);
        
        /// <summary>
        /// Asynchronously retrieves the directory tree structure for the specified directory.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory for which to retrieve the tree structure.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <param name="includeDeletedDocs">Specifies whether to include deleted documents in the directory tree. Set to <see langword="true"/> to
        /// include deleted documents; otherwise, <see langword="false"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DirectoryTreeResponse"/> representing the directory tree structure.</returns>
        Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid directoryId, CancellationToken cancellationToken, bool includeDeletedDocs = false);

        /// <summary>
        /// Asynchronously retrieves a flat list of all documents and subdirectories within the specified directory.
        /// </summary>
        /// <param name="directoryId">The unique identifier of the directory whose contents are to be retrieved.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <param name="includeDeletedDocs">Specifies whether to include documents that have been deleted. Set to <see langword="true"/> to include
        /// deleted documents; otherwise, <see langword="false"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="DirectoryContentsResponse"/> with the flat list of directory contents.</returns>
        Task<DirectoryContentsResponse> GetFlatDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeletedDocs = false);
    }
}
