using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IVacancyRepository : IEntityRepository<Vacancy>
{
    /// <summary>Vacancies for the given posts. Soft-deleted rows excluded.</summary>
    Task<IEnumerable<Vacancy>> GetByPostIdsAsync(IEnumerable<Guid> postIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
