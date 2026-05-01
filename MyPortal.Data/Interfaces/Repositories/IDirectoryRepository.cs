using MyPortal.Contracts.Models.Documents;
using MyPortal.Data.Interfaces.Repositories.Base;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Data.Interfaces.Repositories
{
    public interface IDirectoryRepository : IEntityRepository<Directory>
    {
        Task<DirectoryDetailsResponse?> GetDetailsByIdAsync(Guid directoryId, CancellationToken cancellationToken);

        Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoriesByParentIdAsync(Guid directoryId, CancellationToken cancellationToken);

        Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken);

        Task<bool> IsInSubtreeAsync(Guid rootDirectoryId, Guid candidateDirectoryId,
            CancellationToken cancellationToken);
    }
}
