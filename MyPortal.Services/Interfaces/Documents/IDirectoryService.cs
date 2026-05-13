using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Documents
{
    public interface IDirectoryService
    {
        Task<DirectoryDetailsResponse> CreateAsync(DirectoryUpsertRequest model,
            CancellationToken cancellationToken, IUnitOfWork? uow = null);
        Task<DirectoryDetailsResponse> UpdateAsync(Guid directoryId, DirectoryUpsertRequest model,
            CancellationToken cancellationToken);
        Task DeleteAsync(Guid directoryId, CancellationToken cancellationToken, IUnitOfWork? uow = null, bool softDelete = true);
        Task<DirectoryDetailsResponse> GetDirectoryByIdAsync(Guid directoryId, CancellationToken cancellationToken,
            IUnitOfWork? uow = null);
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
