using System.Data;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.VisibilityScopes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IBulletinRepository : IEntityRepository<Bulletin>
{
    Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken);

    /// <summary>
    /// SP-backed visibility check used by attachment authorisation paths that bypass the
    /// summaries/details SPs. Mirrors the predicate in <c>usp_bulletin_get_details_by_id</c>:
    /// staff pinners see everything; staff creators see their own (even expired); otherwise
    /// the caller must be in the audience AND the bulletin must not be expired.
    /// </summary>
    Task<bool> IsVisibleToUserAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken);

    Task<PageResult<BulletinSummaryResponse>> GetSummariesAsync(BulletinVisibilityScope scope,
        FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the full set of audience rows for a bulletin in a single round-trip.
    /// Caller is responsible for validating the audience list before calling.
    /// </summary>
    Task ReplaceAudiencesAsync(Guid bulletinId, IList<BulletinAudience> audiences,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
