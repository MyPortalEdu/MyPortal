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

public class YearGroupService : BaseService, IYearGroupService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IYearGroupRepository _yearGroupRepository;
    private readonly IStudentGroupService _studentGroupService;

    public YearGroupService(IAuthorizationService authorizationService, ILogger<YearGroupService> logger,
        IUnitOfWorkFactory unitOfWorkFactory, IYearGroupRepository yearGroupRepository,
        IStudentGroupService studentGroupService) : base(authorizationService, logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _yearGroupRepository = yearGroupRepository;
        _studentGroupService = studentGroupService;
    }

    public async Task<PageResult<YearGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure,
            cancellationToken);

        return await _yearGroupRepository.GetSummariesAsync(academicYearId, filter, sort, paging,
            cancellationToken);
    }

    public async Task<YearGroupDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure,
            cancellationToken);

        var result = await _yearGroupRepository.GetDetailsByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException("Year group not found.");

        result.Header.Supervisors = result.Supervisors;
        return result.Header;
    }

    public async Task<Guid> CreateYearGroupAsync(YearGroupUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var studentGroupId = await _studentGroupService.CreateAsync(model.AcademicYearId,
                ToCore(model), model.Supervisors, cancellationToken, uow);

            var yearGroup = new YearGroup
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupId,
                CurriculumYearGroupId = model.CurriculumYearGroupId
            };

            await _yearGroupRepository.InsertAsync(yearGroup, cancellationToken, uow.Transaction);

            return yearGroup.Id;
        }, cancellationToken);
    }

    public async Task UpdateYearGroupAsync(Guid id, YearGroupUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var yearGroup = await GetYearGroupAsync(id, cancellationToken);

            await _studentGroupService.UpdateAsync(yearGroup.StudentGroupId, ToCore(model),
                model.Supervisors, cancellationToken, uow);

            yearGroup.CurriculumYearGroupId = model.CurriculumYearGroupId;
            await _yearGroupRepository.UpdateAsync(yearGroup, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteYearGroupAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var yearGroup = await GetYearGroupAsync(id, cancellationToken);

            // YearGroups.StudentGroupId FKs to StudentGroups, so the YearGroup row must go
            // before the StudentGroup. StudentGroupService.DeleteAsync runs the lock +
            // downstream-data gates at the top, so a failure there rolls back via the UoW.
            await _yearGroupRepository.DeleteAsync(yearGroup.Id, cancellationToken,
                transaction: uow.Transaction);

            await _studentGroupService.DeleteAsync(yearGroup.StudentGroupId, cancellationToken, uow);
        }, cancellationToken);
    }

    private async Task<YearGroup> GetYearGroupAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _yearGroupRepository.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException("Year group not found.");
    }

    private static StudentGroupUpsertCore ToCore(YearGroupUpsertRequest model) => new()
    {
        Code = model.Code,
        Name = model.Name,
        Active = model.Active,
        Notes = model.Notes
    };
}
