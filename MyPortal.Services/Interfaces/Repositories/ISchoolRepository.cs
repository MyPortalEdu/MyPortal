using MyPortal.Contracts.Schools.Queries;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface ISchoolRepository : IEntityRepository<School>
{
    Task<SchoolDetailsDto?> GetLocalSchool(CancellationToken cancellationToken);
}