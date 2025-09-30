using MyPortal.Contracts.Schools.Queries;

namespace MyPortal.Services.Interfaces.Services;

public interface ISchoolService
{
    Task<SchoolDetailsDto> GetLocalSchool(CancellationToken cancellationToken);
}