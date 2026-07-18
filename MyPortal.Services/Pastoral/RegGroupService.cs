using System.Data;
using FluentValidation;
using FluentValidation.Results;
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

public class RegGroupService(
    IAuthorizationService authorizationService,
    ILogger<RegGroupService> logger,
    IUnitOfWorkFactory unitOfWorkFactory,
    IRegGroupRepository regGroupRepository,
    IYearGroupRepository yearGroupRepository,
    IStudentGroupService studentGroupService)
    : BaseService(authorizationService, logger), IRegGroupService
{
    public async Task<PageResult<RegGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure, cancellationToken);

        var result =
            await regGroupRepository.GetSummariesAsync(academicYearId, filter, sort, paging, cancellationToken);

        return result;
    }

    public async Task<RegGroupDetailsResponse> GetDetailsByIdAsync(Guid regGroupId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure, cancellationToken);
        
        var result = await regGroupRepository.GetDetailsByIdAsync(regGroupId, cancellationToken);

        if (result == null)
        {
            throw new NotFoundException("Reg group not found.");
        }

        return result;
    }

    public async Task<Guid> CreateAsync(RegGroupUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        return await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureYearGroupInAcademicYearAsync(model.YearGroupId, model.AcademicYearId,
                cancellationToken, uow.Transaction);

            var studentGroupId = await studentGroupService.CreateAsync(model.AcademicYearId, ToCore(model), model.Supervisors, cancellationToken, uow);

            var regGroup = new RegGroup
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupId,
                YearGroupId = model.YearGroupId,
                RoomId = model.RoomId
            };

            await regGroupRepository.InsertAsync(regGroup, cancellationToken, uow.Transaction);

            return regGroup.Id;

        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid regGroupId, RegGroupUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var regGroup = await GetRegGroupAsync(regGroupId, cancellationToken);

            await EnsureYearGroupInAcademicYearAsync(model.YearGroupId, model.AcademicYearId,
                cancellationToken, uow.Transaction);

            await studentGroupService.UpdateAsync(regGroup.StudentGroupId, ToCore(model), model.Supervisors,
                cancellationToken, uow);

            regGroup.RoomId = model.RoomId;
            regGroup.YearGroupId = model.YearGroupId;
            
            await regGroupRepository.UpdateAsync(regGroup, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid regGroupId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure, cancellationToken);
        
        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var regGroup = await GetRegGroupAsync(regGroupId, cancellationToken);

            await regGroupRepository.DeleteAsync(regGroupId, cancellationToken, transaction: uow.Transaction);

            await studentGroupService.DeleteAsync(regGroup.StudentGroupId, cancellationToken, uow);
        }, cancellationToken);
    }
    
    // A reg group's year group must live in the same academic year as the reg group. Without this
    // a reg group could reference a year group from a different (e.g. prior) academic year, which
    // the DB FK alone can't prevent.
    private async Task EnsureYearGroupInAcademicYearAsync(Guid yearGroupId, Guid academicYearId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        var yearGroupAcademicYearId = await yearGroupRepository.GetAcademicYearIdAsync(yearGroupId,
                                          cancellationToken, transaction)
                                      ?? throw new NotFoundException("Year group not found.");

        if (yearGroupAcademicYearId != academicYearId)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(RegGroupUpsertRequest.YearGroupId),
                    "The selected year group belongs to a different academic year.")
            });
        }
    }

    private async Task<RegGroup> GetRegGroupAsync(Guid id, CancellationToken cancellationToken)
    {
        return await regGroupRepository.GetByIdAsync(id, cancellationToken)
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