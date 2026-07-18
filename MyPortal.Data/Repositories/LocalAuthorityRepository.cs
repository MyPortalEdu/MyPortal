using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class LocalAuthorityRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<LocalAuthority>(factory, authorizationService), ILocalAuthorityRepository
{
    public async Task<PageResult<LocalAuthoritySummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("School.GetLocalAuthoritySummaries.sql");

        return await GetListPagedAsync<LocalAuthoritySummaryResponse>(sql, null, filter, sort, paging, false,
            cancellationToken);
    }
}
