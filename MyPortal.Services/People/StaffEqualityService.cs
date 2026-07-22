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
/// Staff-flavoured wrapper over <see cref="IPersonEqualityService"/>: enforces staff access under
/// <see cref="StaffArea.EqualityDetails"/> (HR-only edit, self/HR view — no Managed scope),
/// resolves the staff member to a person, delegates the person fields, and owns the staff-level
/// disability declaration (flag + free text) and the multi-select disability links.
/// </summary>
public class StaffEqualityService(
    IAuthorizationService authorizationService,
    ILogger<StaffEqualityService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IPersonEqualityService personEqualityService,
    IDisabilityRepository disabilityRepository,
    IStaffMemberDisabilityRepository staffMemberDisabilityRepository,
    IImpairmentEffectRepository impairmentEffectRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffEqualityService
{
    public async Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        // Special-category data: HR (All) or the person themselves (Own). No Managed scope.
        await accessService.RequireAsync(staffMemberId, StaffArea.EqualityDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var response = await personEqualityService.GetEqualityDetailsAsync(staffMember.PersonId, cancellationToken);

        // Staff-level disability bits the person service doesn't own.
        response.HasDisability = staffMember.HasDisability;
        response.DisabilityDetails = staffMember.DisabilityDetails;
        response.ImpairmentEffectId = staffMember.ImpairmentEffectId;
        response.DisabilityNumber = staffMember.DisabilityNumber;

        var links = await staffMemberDisabilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        response.DeclaredDisabilities = links
            .Select(l => new StaffDisabilityResponse
            {
                DisabilityId = l.DisabilityId,
                DateAdvised = l.DateAdvised,
                IsLongTerm = l.IsLongTerm,
                AffectsWorkingAbility = l.AffectsWorkingAbility,
                AssistanceRequired = l.AssistanceRequired
            })
            .ToList();

        var disabilities = await disabilityRepository.GetListAsync(cancellationToken: cancellationToken);
        response.Disabilities = disabilities.ToOrderedLookup();

        var impairmentEffects = await impairmentEffectRepository.GetListAsync(cancellationToken: cancellationToken);
        response.ImpairmentEffects = impairmentEffects.ToOrderedLookup();

        return response;
    }

    public async Task UpdateEqualityDetailsAsync(Guid staffMemberId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        // Equality data is HR-edit-only — no Own/Managed edit scope.
        await accessService.RequireAsync(staffMemberId, StaffArea.EqualityDetails, StaffAccess.EditAll,
            cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        staffMember.HasDisability = model.HasDisability;
        staffMember.DisabilityDetails = model.DisabilityDetails;
        staffMember.ImpairmentEffectId = model.ImpairmentEffectId;
        staffMember.DisabilityNumber = model.DisabilityNumber;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await personEqualityService.UpdateEqualityDetailsAsync(staffMember.PersonId, model, cancellationToken, uow);
            await staffMemberRepository.UpdateAsync(staffMember, cancellationToken, uow.Transaction);
            await ReconcileDisabilitiesAsync(staffMemberId, model.DeclaredDisabilities, uow.Transaction,
                cancellationToken);
        }, cancellationToken);
    }

    // Matched on DisabilityId — the same disability can't be declared twice, so it's the natural
    // key. Matched rows have their Equality Act detail updated in place.
    private async Task ReconcileDisabilitiesAsync(Guid staffMemberId, List<StaffDisabilityUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var wanted = incoming
            .GroupBy(i => i.DisabilityId)
            .ToDictionary(g => g.Key, g => g.First());

        var existing =
            (await staffMemberDisabilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken,
                transaction)).ToList();

        // Hard-delete links the payload dropped (the join row carries no soft-delete column).
        foreach (var row in existing.Where(row => !wanted.ContainsKey(row.DisabilityId)))
        {
            await staffMemberDisabilityRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        var existingByDisability = existing.ToDictionary(e => e.DisabilityId);

        foreach (var (disabilityId, item) in wanted)
        {
            if (existingByDisability.TryGetValue(disabilityId, out var entity))
            {
                ApplyDisability(entity, item);
                await staffMemberDisabilityRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var link = new StaffMemberDisability
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId,
                    DisabilityId = disabilityId
                };
                ApplyDisability(link, item);
                await staffMemberDisabilityRepository.InsertAsync(link, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyDisability(StaffMemberDisability entity, StaffDisabilityUpsertItem item)
    {
        entity.DateAdvised = item.DateAdvised;
        entity.IsLongTerm = item.IsLongTerm;
        entity.AffectsWorkingAbility = item.AffectsWorkingAbility;
        entity.AssistanceRequired = item.AssistanceRequired;
    }
}
