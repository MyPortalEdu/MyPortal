using MyPortal.Contracts.Schools.Queries;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface ISchoolRepository : IEntityRepository<School>
{
    Task<SchoolDetailsDto?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
}