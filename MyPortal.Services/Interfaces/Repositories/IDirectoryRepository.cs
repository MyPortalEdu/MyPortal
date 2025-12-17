using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Interfaces.Repositories.Base;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IDirectoryRepository : IEntityRepository<Directory>
    {
        Task<DirectoryDetailsResponse?> GetDetailsByIdAsync(Guid directoryId, CancellationToken cancellationToken);
        Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoriesByParentIdAsync(Guid directoryId, CancellationToken cancellationToken);

        Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken);
    }
}
