using System.Data;

namespace MyPortal.Common.Interfaces;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> BeginAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
