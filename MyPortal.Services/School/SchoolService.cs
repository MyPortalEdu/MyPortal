using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.School;

public class SchoolService : BaseService, ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;

    public SchoolService(IAuthorizationService authorizationService, ISchoolRepository schoolRepository) : base(
        authorizationService)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetLocalSchoolAsync(cancellationToken);
    }

    public async Task<SchoolDetailsResponse?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetDetailsByIdAsync(id,  cancellationToken);
    }
}