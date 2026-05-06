using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Documents
{
    public interface IDirectoryService
    {
        Task<DirectoryDetailsResponse> CreateDirectoryAsync(DirectoryUpsertRequest model,
            CancellationToken cancellationToken, IUnitOfWork? uow = null);
        Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid directoryId, DirectoryUpsertRequest model,
            CancellationToken cancellationToken);
        Task DeleteDirectoryAsync(Guid directoryId, CancellationToken cancellationToken, IUnitOfWork? uow = null);
        Task<DirectoryDetailsResponse> GetDirectoryByIdAsync(Guid directoryId, CancellationToken cancellationToken);
        Task<DirectoryDetailsResponse?> TryGetDirectoryByIdAsync(Guid directoryId,
            CancellationToken cancellationToken);
        Task<DirectoryContentsResponse>
            GetDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken);
        Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid directoryId, CancellationToken cancellationToken, bool includeDeletedDocs = false);
        Task<DirectoryContentsResponse> GetFlatDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeletedDocs = false);
        Task<bool> IsDirectoryInSubtreeAsync(Guid rootDirectoryId, Guid candidateDirectoryId,
            CancellationToken cancellationToken);
    }
}
