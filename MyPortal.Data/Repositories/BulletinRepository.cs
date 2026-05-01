using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Repositories;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Data.VisibilityScopes;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class BulletinRepository : EntityRepository<Bulletin>, IBulletinRepository
{
    public BulletinRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }

    public async Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = "[dbo].[sp_bulletin_get_details_by_id]";

        var p = new
        {
            bulletinId,
            currentUserId = scope.CurrentUserId,
            isStaff = scope.IsStaff,
            canView = scope.CanView,
            canEdit = scope.CanEdit,
            canApprove = scope.CanApprove,
            nowUtc = DateTime.UtcNow
        };

        var result =
            await conn.ExecuteStoredProcedureAsync<BulletinDetailsResponse>(sql, p,
                cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<PageResult<BulletinSummaryResponse>> GetSummariesAsync(BulletinVisibilityScope scope,
        FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Bulletins.GetBulletinSummaries.sql");

        var p = scope.ToSqlParams(DateTime.UtcNow);

        var result =
            await GetListPagedAsync<BulletinSummaryResponse>(sql, p, filter, sort, paging, false, cancellationToken);

        return result;
    }
}