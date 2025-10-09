using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.Schools.Queries;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.Services;

public class SchoolService : BaseService, ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;

    public SchoolService(IAuthorizationService authorizationService, ISchoolRepository schoolRepository) : base(
        authorizationService)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<SchoolDetailsDto?> GetLocalSchoolAsync(CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolDetails, cancellationToken);
        
        return await _schoolRepository.GetLocalSchoolAsync(cancellationToken);
    }

    public async Task<SchoolDetailsDto?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetDetailsByIdAsync(id,  cancellationToken);
    }
}