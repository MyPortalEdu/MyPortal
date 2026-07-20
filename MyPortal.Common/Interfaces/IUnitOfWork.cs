using System.Data;

namespace MyPortal.Common.Interfaces;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
    void OnCommitted(Func<CancellationToken, Task> action);
}
