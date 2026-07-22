using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IVacancyRepository : IEntityRepository<Vacancy>
{
    Task<IEnumerable<Vacancy>> GetByPostIdsAsync(IEnumerable<Guid> postIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
