using MyPortal.Contracts.Models.School;

namespace MyPortal.Services.Interfaces.Services;

public interface ISchoolService
{
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    Task<SchoolDetailsResponse?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken);
}