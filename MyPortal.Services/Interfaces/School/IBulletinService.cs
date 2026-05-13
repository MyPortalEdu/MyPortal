using MyPortal.Contracts.Models.Bulletins;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.School;

public interface IBulletinService
{
    Task<BulletinDetailsResponse> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken);

    Task<PageResult<BulletinSummaryResponse>> GetBulletinSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(BulletinUpsertRequest model, CancellationToken cancellationToken);

    Task UpdateAsync(Guid bulletinId, BulletinUpsertRequest model, CancellationToken cancellationToken);

    Task DeleteAsync(Guid bulletinId, CancellationToken cancellationToken);

    Task UpdatePinAsync(Guid bulletinId, bool isPinned, long expectedVersion, CancellationToken cancellationToken);

    /// <summary>
    /// Records the current caller's acknowledgement of <paramref name="bulletinId"/>.
    /// Idempotent: re-acknowledging is a no-op. Throws NotFound if the bulletin is not
    /// visible to the caller, and InvalidOperationException if the bulletin does not
    /// require acknowledgement.
    /// </summary>
    Task AcknowledgeAsync(Guid bulletinId, CancellationToken cancellationToken);
}
