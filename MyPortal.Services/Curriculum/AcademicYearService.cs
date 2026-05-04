using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Curriculum;

namespace MyPortal.Services.Curriculum;

public class AcademicYearService : BaseService, IAcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;

    public AcademicYearService(IAuthorizationService authorizationService, ILogger<AcademicYearService> logger,
        IAcademicYearRepository academicYearRepository) : base(authorizationService, logger)
    {
        _academicYearRepository = academicYearRepository;
    }
}