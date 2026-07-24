using System.Transactions;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Services;

public class BaseService(IAuthorizationService authorizationService, ILogger<BaseService> logger)
{
    protected IAuthorizationService AuthorizationService { get; } = authorizationService;
    protected ILogger<BaseService> Logger { get; } = logger;
    
    protected static TransactionScope CreateTransactionScope()
    {
        return new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}
