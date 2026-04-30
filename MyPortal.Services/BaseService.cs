using System.Transactions;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Services;

public class BaseService
{
    protected IAuthorizationService AuthorizationService { get; }
    protected ILogger<BaseService> Logger { get; }

    public BaseService(IAuthorizationService authorizationService, ILogger<BaseService> logger)
    {
        AuthorizationService = authorizationService;
        Logger = logger;
    }
    
    protected static TransactionScope CreateTransactionScope()
    {
        return new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}