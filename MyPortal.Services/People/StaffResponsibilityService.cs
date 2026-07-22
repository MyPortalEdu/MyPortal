using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the Responsibilities area (DSL, First Aider, SENCO, …). Gating is
/// <see cref="StaffArea.Responsibilities"/> — self / line-manager / HR view, HR-only edit. The save
/// is a whole-area reconcile: new rows inserted, matched ids updated, dropped rows soft-deleted.
/// </summary>
public class StaffResponsibilityService(
    IAuthorizationService authorizationService,
    ILogger<StaffResponsibilityService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IStaffResponsibilityRepository responsibilityRepository,
    IStaffResponsibilityTypeRepository responsibilityTypeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffResponsibilityService
{
    public async Task<StaffResponsibilitiesResponse> GetResponsibilitiesAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.Responsibilities,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var responsibilities = await responsibilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var types = await responsibilityTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffResponsibilitiesResponse
        {
            // Current first, then most-recently started.
            Responsibilities = responsibilities
                .OrderBy(r => r.EndDate.HasValue)
                .ThenByDescending(r => r.StartDate)
                .Select(MapResponsibility)
                .ToList(),
            ResponsibilityTypes = types.ToOrderedLookup()
        };
    }

    public async Task UpdateResponsibilitiesAsync(Guid staffMemberId, StaffResponsibilitiesUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.Responsibilities, StaffAccess.EditAll,
            cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileAsync(staffMemberId, model.Responsibilities, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileAsync(Guid staffMemberId, List<StaffResponsibilityUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await responsibilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await responsibilityRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                Apply(entity, item);
                await responsibilityRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffResponsibility
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId
                };
                Apply(created, item);
                await responsibilityRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void Apply(StaffResponsibility entity, StaffResponsibilityUpsertItem item)
    {
        entity.ResponsibilityTypeId = item.ResponsibilityTypeId;
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        entity.Notes = item.Notes;
    }

    private static StaffResponsibilityResponse MapResponsibility(StaffResponsibility r) => new()
    {
        Id = r.Id,
        ResponsibilityTypeId = r.ResponsibilityTypeId,
        StartDate = r.StartDate,
        EndDate = r.EndDate,
        Notes = r.Notes
    };
}
