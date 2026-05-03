using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISchoolRepository : IEntityRepository<Core.Entities.School>
{
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsResponse?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken);
}