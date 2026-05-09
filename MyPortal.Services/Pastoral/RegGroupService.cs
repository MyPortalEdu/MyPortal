using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

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

    public async Task<PageResult<RegGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure, cancellationToken);

        var result =
            await _regGroupRepository.GetSummariesAsync(academicYearId, filter, sort, paging, cancellationToken);

        return result;
    }

    public async Task<RegGroupDetailsResponse> GetDetailsByIdAsync(Guid regGroupId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure, cancellationToken);
        
        var result = await _regGroupRepository.GetDetailsByIdAsync(regGroupId, cancellationToken);

        if (result == null)
        {
            throw new NotFoundException("Reg group not found.");
        }

        return result;
    }

    public async Task<Guid> CreateAsync(RegGroupUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var studentGroupId = await _studentGroupService.CreateAsync(model.AcademicYearId, ToCore(model), model.Supervisors, cancellationToken, uow);

            var regGroup = new RegGroup
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupId,
                YearGroupId = model.YearGroupId,
                RoomId = model.RoomId
            };

            await _regGroupRepository.InsertAsync(regGroup, cancellationToken, uow.Transaction);

            return regGroup.Id;

        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid regGroupId, RegGroupUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var regGroup = await GetRegGroupAsync(regGroupId, cancellationToken);

            await _studentGroupService.UpdateAsync(regGroup.StudentGroupId, ToCore(model), model.Supervisors,
                cancellationToken, uow);
            
            regGroup.RoomId = model.RoomId;
            regGroup.YearGroupId = model.YearGroupId;
            
            await _regGroupRepository.UpdateAsync(regGroup, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid regGroupId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var regGroup = await GetRegGroupAsync(regGroupId, cancellationToken);

            await _regGroupRepository.DeleteAsync(regGroupId, cancellationToken, transaction: uow.Transaction);

            await _studentGroupService.DeleteAsync(regGroup.StudentGroupId, cancellationToken, uow);
        }, cancellationToken);
    }
    
    private async Task<RegGroup> GetRegGroupAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _regGroupRepository.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException("Year group not found.");
    }
    
    private static StudentGroupUpsertCore ToCore(RegGroupUpsertRequest model) => new()
    {
        Code = model.Code,
        Name = model.Name,
        Active = model.Active,
        Notes = model.Notes
    };
}