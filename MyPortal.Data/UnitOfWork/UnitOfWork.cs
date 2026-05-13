using System.Data;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.UnitOfWork;

internal sealed class UnitOfWork : IUnitOfWork
{
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _completed;
    private readonly List<Func<CancellationToken, Task>> _postCommit = new();

    public UnitOfWork(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public IDbConnection Connection =>
        _connection ?? throw new ObjectDisposedException(nameof(UnitOfWork));

    public IDbTransaction Transaction =>
        _transaction ?? throw new ObjectDisposedException(nameof(UnitOfWork));

    public void OnCommitted(Func<CancellationToken, Task> action)
    {
        if (_completed)
        {
            throw new InvalidOperationException(
                "Cannot register a post-commit action after the unit of work has completed.");
        }
        _postCommit.Add(action);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            throw new InvalidOperationException("Unit of work has already been committed or rolled back.");
        }

        Transaction.Commit();
        _completed = true;

        // Post-commit actions run after the DB transaction is durable. A failure here
        // can't undo the commit, so each action is responsible for its own logging /
        // error handling — we just don't want one failure to swallow the rest.
        foreach (var action in _postCommit)
        {
            try
            {
                await action(cancellationToken);
            }
            catch
            {
                // Caller logs inside its own action; nothing useful to do at this layer.
            }
        }
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_completed)
        {
            return Task.CompletedTask;
        }

        Transaction.Rollback();
        _completed = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // Auto-rollback on dispose without explicit commit — guards against
        // accidentally committing partial work if a delegate throws.
        if (!_completed && _transaction is not null)
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // Swallow: the connection may already be in a bad state; nothing useful to do here.
            }
        }

        _transaction?.Dispose();
        _transaction = null;
        _connection?.Dispose();
        _connection = null;

        return ValueTask.CompletedTask;
    }
}
