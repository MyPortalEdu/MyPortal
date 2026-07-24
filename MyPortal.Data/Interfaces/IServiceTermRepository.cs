using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IServiceTermRepository : IEntityRepository<ServiceTerm>
{
    Task<IEnumerable<ServiceTermRow>> GetAllWithUsageAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task<bool> CodeExistsAsync(string code, Guid? excludeServiceTermId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
