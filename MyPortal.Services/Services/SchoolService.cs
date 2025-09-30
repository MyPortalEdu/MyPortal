using MyPortal.Contracts.Schools.Queries;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.Services;

public class SchoolService : ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;
    
    public SchoolService(ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }


    public async Task<SchoolDetailsDto?> GetLocalSchool(CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetLocalSchool(cancellationToken);
    }
}