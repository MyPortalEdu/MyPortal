using System.Data;
using MyPortal.Common.Interfaces;

namespace MyPortal.Services.Extensions;

public static class UnitOfWorkFactoryExtensions
{
    /// <summary>
    /// Joins an existing UoW or starts a new one for the duration of <paramref name="work"/>.
    /// When <paramref name="uow"/> is null a fresh UoW is opened, the delegate runs, and the
    /// UoW is committed on success / rolled back on exception. When non-null the caller's
    /// UoW is used as-is and commit/rollback stays the caller's responsibility.
    /// </summary>
    public static async Task<T> RunInTransactionAsync<T>(this IUnitOfWorkFactory factory,
        IUnitOfWork? uow,
        Func<IUnitOfWork, Task<T>> work,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (uow is not null)
        {
            return await work(uow);
        }

        await using var ownedUow = await factory.BeginAsync(isolationLevel, cancellationToken);
        var result = await work(ownedUow);
        await ownedUow.CommitAsync(cancellationToken);
        return result;
    }

    public static async Task RunInTransactionAsync(this IUnitOfWorkFactory factory,
        IUnitOfWork? uow,
        Func<IUnitOfWork, Task> work,
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (uow is not null)
        {
            await work(uow);
            return;
        }

        await using var ownedUow = await factory.BeginAsync(isolationLevel, cancellationToken);
        await work(ownedUow);
        await ownedUow.CommitAsync(cancellationToken);
    }
}
