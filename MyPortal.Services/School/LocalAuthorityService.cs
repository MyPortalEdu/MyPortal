using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.School;

public class LocalAuthorityService : BaseService, ILocalAuthorityService
{
    private readonly ILocalAuthorityRepository _repository;

    public LocalAuthorityService(IAuthorizationService authorizationService, ILogger<BaseService> logger,
        ILocalAuthorityRepository repository) : base(authorizationService, logger)
    {
        _repository = repository;
    }

    public async Task<PageResult<LocalAuthoritySummaryResponse>> GetSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.ViewAgencies, cancellationToken);

        return await _repository.GetSummariesAsync(filter, sort, paging, cancellationToken);
    }
}
