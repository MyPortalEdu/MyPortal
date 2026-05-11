using System.Data;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.UnitOfWork;

public sealed class UnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UnitOfWorkFactory(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<IUnitOfWork> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = _connectionFactory.Create();
        try
        {
            connection.Open();
            var transaction = connection.BeginTransaction(isolationLevel);
            return Task.FromResult<IUnitOfWork>(new UnitOfWork(connection, transaction));
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
