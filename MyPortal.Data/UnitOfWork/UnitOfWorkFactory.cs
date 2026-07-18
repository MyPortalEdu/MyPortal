using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.UnitOfWork;

public sealed class UnitOfWorkFactory(IDbConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    : IUnitOfWorkFactory
{
    private readonly ILogger<UnitOfWork> _logger = loggerFactory.CreateLogger<UnitOfWork>();

    public Task<IUnitOfWork> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = connectionFactory.Create();
        try
        {
            connection.Open();
            var transaction = connection.BeginTransaction(isolationLevel);
            return Task.FromResult<IUnitOfWork>(new UnitOfWork(connection, transaction, _logger));
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
