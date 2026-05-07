using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Pastoral;

namespace MyPortal.Services.Pastoral;

public class RegGroupService : BaseService, IRegGroupService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IRegGroupRepository _regGroupRepository;
    private readonly IStudentGroupService _studentGroupService;

    public RegGroupService(IAuthorizationService authorizationService, ILogger<RegGroupService> logger,
        IUnitOfWorkFactory unitOfWorkFactory, IRegGroupRepository regGroupRepository,
        IStudentGroupService studentGroupService) : base(
        authorizationService, logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _regGroupRepository = regGroupRepository;
        _studentGroupService = studentGroupService;
    }
}