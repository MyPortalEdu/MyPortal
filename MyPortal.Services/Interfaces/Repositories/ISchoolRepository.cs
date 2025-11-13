using MyPortal.Contracts.Models.Schools;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface ISchoolRepository : IEntityRepository<School>
{
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsResponse?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken);
}