using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class BulletinRepository : EntityRepository<Bulletin>, IBulletinRepository
{
    protected BulletinRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }

    public async Task<BulletinDetailsDto?> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = "[dbo].[sp_bulletin_get_details_by_id]";

        var p = new { bulletinId };
        
        var result = await conn.ExecuteStoredProcedureAsync<BulletinDetailsDto>(sql, p, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<PageResult<BulletinSummaryDto>> GetBulletinsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Bulletins.GetBulletinSummaries.sql");

        var result =
            await GetListPagedAsync<BulletinSummaryDto>(sql, null, filter, sort, paging, false, cancellationToken);

        return result;
    }
}