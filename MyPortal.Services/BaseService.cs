using System.Transactions;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;

namespace MyPortal.Services;

public class BaseService(IAuthorizationService authorizationService, ILogger<BaseService> logger)
{
    protected IAuthorizationService AuthorizationService { get; } = authorizationService;
    protected ILogger<BaseService> Logger { get; } = logger;

    /// <summary>
    /// Ambient TransactionScope for code paths that have to bridge ASP.NET Identity calls
    /// (RoleManager / UserManager) with our own repos. Identity's manager APIs don't accept
    /// an IDbTransaction, so we can't thread a UoW through them — TransactionScope is the
    /// only way to make those mixed flows atomic. Everything else should use IUnitOfWork.
    /// </summary>
    protected static TransactionScope CreateTransactionScope()
    {
        return new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}
