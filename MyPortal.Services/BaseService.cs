using MyPortal.Auth.Interfaces;
using QueryKit.Repositories.Enums;
using QueryKit.Repositories.Filtering;

namespace MyPortal.Services;

public class BaseService
{
    protected readonly IAuthorizationService _authorizationService;

    public BaseService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }
    
    protected FilterOptions ApplyFilterCriteria(FilterOptions? filter, BoolJoin join, params FilterCriterion[] criteria)
    {
        var groups = filter?.Groups.ToList() ?? [];

        var filterGroup = new FilterGroup
        {
            Criteria = criteria,
            Join = join
        };
        
        groups.Add(filterGroup);

        return new FilterOptions
        {
            Groups = groups.ToArray(),
            Join = BoolJoin.And
        };
    }
}