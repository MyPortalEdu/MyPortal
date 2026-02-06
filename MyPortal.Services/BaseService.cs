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
}