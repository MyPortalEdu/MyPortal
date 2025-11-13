using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Services
{
    public interface IDirectoryService
    {
        Task<DirectoryDetailsResponse> CreateDirectoryAsync(DirectoryUpsertRequest model,
            CancellationToken cancellationToken);

        Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid directoryId, DirectoryUpsertRequest model,
            CancellationToken cancellationToken);

        Task DeleteDirectoryAsync(Guid directoryId, CancellationToken cancellationToken);

        Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid directoryId, CancellationToken cancellationToken);

        Task<DirectoryContentsResponse>
            GetDirectoryContentsAsync(Guid directoryId, CancellationToken cancellationToken);
    }
}
