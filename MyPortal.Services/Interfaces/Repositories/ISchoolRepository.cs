using MyPortal.Contracts.Models.Schools.Queries;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface ISchoolRepository : IEntityRepository<School>
{
    Task<SchoolDetailsDto?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsDto?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken);
}