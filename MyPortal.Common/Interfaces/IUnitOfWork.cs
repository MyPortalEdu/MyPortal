using System.Data;

namespace MyPortal.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);

    // Queue an action to run after the transaction commits successfully. Used for
    // side effects that must not happen if the DB work rolls back — e.g. purging a
    // blob whose row was hard-deleted inside the txn. Dropped on rollback/dispose.
    void OnCommitted(Func<CancellationToken, Task> action);
}
