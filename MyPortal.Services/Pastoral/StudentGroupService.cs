using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
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

public class StudentGroupService(
    IAuthorizationService authorizationService,
    ILogger<StudentGroupService> logger,
    IUnitOfWorkFactory unitOfWorkFactory,
    IStudentGroupRepository studentGroupRepository,
    IStudentGroupSupervisorRepository studentGroupSupervisorRepository,
    IAcademicYearRepository academicYearRepository)
    : BaseService(authorizationService, logger), IStudentGroupService
{
    public async Task<PageResult<StudentGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        // Read access matches the rest of the pastoral structure rather than carving
        // out a new permission — anyone who can already see houses/year groups/reg
        // groups can browse the unified listing too.
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure,
            cancellationToken);

        return await studentGroupRepository.GetSummariesAsync(academicYearId, filter, sort, paging,
            cancellationToken);
    }

    public async Task<Guid> CreateAsync(Guid academicYearId, StudentGroupUpsertCore core,
        IList<StudentGroupSupervisorUpsertRequest> supervisors,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        return await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            await EnsureAcademicYearNotLockedAsync(academicYearId, "created", cancellationToken,
                ownedUow.Transaction);

            await EnsureCodeUniqueAsync(academicYearId, core.Code, excludeStudentGroupId: null,
                cancellationToken, ownedUow.Transaction);

            var studentGroup = new StudentGroup
            {
                Id = SqlConvention.SequentialGuid(),
                AcademicYearId = academicYearId,
                Description = core.Name,
                Code = core.Code,
                Active = core.Active,
                Notes = core.Notes
            };

            await studentGroupRepository.InsertAsync(studentGroup, cancellationToken, ownedUow.Transaction);

            var mainSupervisorId = await ReplaceSupervisorsAsync(studentGroup.Id, supervisors,
                cancellationToken, ownedUow.Transaction);

            if (mainSupervisorId.HasValue)
            {
                studentGroup.MainSupervisorId = mainSupervisorId;
                await studentGroupRepository.UpdateAsync(studentGroup, cancellationToken,
                    ownedUow.Transaction);
            }

            return studentGroup.Id;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid studentGroupId, StudentGroupUpsertCore core,
        IList<StudentGroupSupervisorUpsertRequest> supervisors,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            var studentGroup = await GetOrThrowAsync(studentGroupId, cancellationToken,
                ownedUow.Transaction);

            await EnsureAcademicYearNotLockedAsync(studentGroup.AcademicYearId, "edited",
                cancellationToken, ownedUow.Transaction);

            await EnsureCodeUniqueAsync(studentGroup.AcademicYearId, core.Code,
                excludeStudentGroupId: studentGroupId, cancellationToken, ownedUow.Transaction);

            // StudentGroup.MainSupervisorId -> StudentGroupSupervisor.Id is a circular FK with
            // StudentGroupSupervisor.StudentGroupId -> StudentGroup.Id. Null the back-reference
            // before deleting existing supervisors so the FK doesn't block the delete, then
            // restore it after the new set is inserted.
            if (studentGroup.MainSupervisorId.HasValue)
            {
                studentGroup.MainSupervisorId = null;
                await studentGroupRepository.UpdateAsync(studentGroup, cancellationToken,
                    ownedUow.Transaction);
            }

            var mainSupervisorId = await ReplaceSupervisorsAsync(studentGroup.Id, supervisors,
                cancellationToken, ownedUow.Transaction);

            studentGroup.Description = core.Name;
            studentGroup.Code = core.Code;
            studentGroup.Active = core.Active;
            studentGroup.Notes = core.Notes;
            studentGroup.MainSupervisorId = mainSupervisorId;

            await studentGroupRepository.UpdateAsync(studentGroup, cancellationToken,
                ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid studentGroupId,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            var studentGroup = await GetOrThrowAsync(studentGroupId, cancellationToken,
                ownedUow.Transaction);

            await EnsureAcademicYearNotLockedAsync(studentGroup.AcademicYearId, "deleted",
                cancellationToken, ownedUow.Transaction);

            // StudentGroup is not soft-deletable, so any FK reference (memberships, marksheets,
            // curriculum groups, activities, parent-evening groups, result-set releases) would
            // fail the delete. Surface a friendly error before the constraint fires.
            if (await studentGroupRepository.HasDownstreamDataAsync(studentGroup.Id,
                    cancellationToken, ownedUow.Transaction))
            {
                throw new AcademicYearLockedException(
                    "This group has data attached to it and cannot be deleted. " +
                    "Remove memberships and any group-level data first.");
            }

            // Break the circular FK before deleting supervisors (see UpdateAsync).
            if (studentGroup.MainSupervisorId.HasValue)
            {
                studentGroup.MainSupervisorId = null;
                await studentGroupRepository.UpdateAsync(studentGroup, cancellationToken,
                    ownedUow.Transaction);
            }

            var existingSupervisors = await studentGroupSupervisorRepository.GetByStudentGroupAsync(
                studentGroup.Id, cancellationToken, ownedUow.Transaction);

            foreach (var supervisor in existingSupervisors)
            {
                await studentGroupSupervisorRepository.DeleteAsync(supervisor.Id, cancellationToken,
                    transaction: ownedUow.Transaction);
            }

            await studentGroupRepository.DeleteAsync(studentGroup.Id, cancellationToken,
                transaction: ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task EnsureAcademicYearNotLockedAsync(Guid academicYearId, string verb,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var academicYear = await academicYearRepository.GetByIdAsync(academicYearId, cancellationToken,
                               transaction)
                           ?? throw new NotFoundException("Academic year not found.");

        if (academicYear.IsLocked)
        {
            throw new AcademicYearLockedException(
                $"This academic year is locked and groups cannot be {verb}.");
        }
    }

    // Codes must be unique per academic year across the whole pastoral hierarchy (houses,
    // year groups and reg groups share the StudentGroups table). The DB has no unique index,
    // so guard here and surface a friendly error instead of a duplicate row.
    private async Task EnsureCodeUniqueAsync(Guid academicYearId, string code, Guid? excludeStudentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        if (await studentGroupRepository.CodeExistsAsync(academicYearId, code, excludeStudentGroupId,
                cancellationToken, transaction))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Code",
                    $"A group with code '{code}' already exists in this academic year.")
            });
        }
    }

    private async Task<StudentGroup> GetOrThrowAsync(Guid studentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        return await studentGroupRepository.GetByIdAsync(studentGroupId, cancellationToken, transaction)
               ?? throw new NotFoundException("Student group not found.");
    }

    // Deletes any existing supervisors for the group, inserts the new set, and returns the
    // chosen MainSupervisor.Id (or null if none flagged). Throws if more than one supervisor
    // is flagged as the main supervisor.
    private async Task<Guid?> ReplaceSupervisorsAsync(Guid studentGroupId,
        IList<StudentGroupSupervisorUpsertRequest> supervisorModels,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        if (supervisorModels.Count(s => s.MainSupervisor) > 1)
        {
            throw new InvalidOperationException("Only one supervisor can be flagged as the main supervisor.");
        }

        // The (StudentGroupId, SupervisorId) pair has no unique constraint, so a repeated staff
        // member would create duplicate supervisor rows. Reject it up front.
        if (supervisorModels.GroupBy(s => s.StaffMemberId).Any(g => g.Count() > 1))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Supervisors",
                    "A staff member cannot be added as a supervisor more than once.")
            });
        }

        var existing = await studentGroupSupervisorRepository.GetByStudentGroupAsync(studentGroupId,
            cancellationToken, transaction);

        foreach (var supervisor in existing)
        {
            await studentGroupSupervisorRepository.DeleteAsync(supervisor.Id, cancellationToken,
                transaction: transaction);
        }

        Guid? mainSupervisorId = null;

        foreach (var supervisorModel in supervisorModels)
        {
            var supervisor = new StudentGroupSupervisor
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupId,
                SupervisorId = supervisorModel.StaffMemberId,
                Title = supervisorModel.Title
            };

            await studentGroupSupervisorRepository.InsertAsync(supervisor, cancellationToken, transaction);

            if (supervisorModel.MainSupervisor)
            {
                mainSupervisorId = supervisor.Id;
            }
        }

        return mainSupervisorId;
    }
}
