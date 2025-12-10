using MyPortal.Contracts.Models.School;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface ISchoolRepository : IEntityRepository<Core.Entities.School>
{
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsResponse?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken);
}