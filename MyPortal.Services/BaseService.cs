using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Services;

public class BaseService
{
    protected IAuthorizationService AuthorizationService { get; }
    protected ILogger Logger { get; }

    public BaseService(IAuthorizationService authorizationService, ILogger<BaseService> logger)
    {
        AuthorizationService = authorizationService;
        Logger = logger;
    }
}