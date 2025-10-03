using MyPortal.Contracts.Models.Schools.Queries;

namespace MyPortal.Services.Interfaces.Services;

public interface ISchoolService
{
    Task<SchoolDetailsDto?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsDto?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken);
}