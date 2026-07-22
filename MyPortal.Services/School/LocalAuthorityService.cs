using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.School;

public class LocalAuthorityService(
    IAuthorizationService authorizationService,
    ILogger<BaseService> logger,
    ILocalAuthorityRepository repository)
    : BaseService(authorizationService, logger), ILocalAuthorityService
{
    public async Task<PageResult<LocalAuthoritySummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.ViewAgencies, cancellationToken);

        return await repository.GetSummariesAsync(filter, sort, paging, cancellationToken);
    }
}
