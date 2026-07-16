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
/// Owns the Absences &amp; Leave area. Gating is enforced under <see cref="StaffArea.Absences"/> —
/// self / line-manager / HR view, line-manager / HR edit (no self-edit). Confidential absences are
/// visible only to HR (All scope) and the staff member themselves; a line manager (Managed scope)
/// never sees them, and a Managed editor's whole-area save is scoped to non-confidential rows so it
/// can never delete or alter the confidential ones it can't see.
/// </summary>
public class StaffAbsenceService : BaseService, IStaffAbsenceService
{
    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IStaffAbsenceRepository _absenceRepository;
    private readonly IStaffAbsenceTypeRepository _absenceTypeRepository;
    private readonly IStaffIllnessTypeRepository _illnessTypeRepository;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffAbsenceService(IAuthorizationService authorizationService, ILogger<StaffAbsenceService> logger,
        IStaffMemberAccessService accessService, IStaffMemberRepository staffMemberRepository,
        IStaffAbsenceRepository absenceRepository, IStaffAbsenceTypeRepository absenceTypeRepository,
        IStaffIllnessTypeRepository illnessTypeRepository, IValidationService validationService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _absenceRepository = absenceRepository;
        _absenceTypeRepository = absenceTypeRepository;
        _illnessTypeRepository = illnessTypeRepository;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<StaffAbsencesResponse> GetAbsencesAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var canSeeConfidential = await CanSeeConfidentialAsync(staffMemberId, cancellationToken);

        var absences = await _absenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);

        if (!canSeeConfidential)
        {
            absences = absences.Where(a => !a.IsConfidential);
        }

        var absenceTypes = await _absenceTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var illnessTypes = await _illnessTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffAbsencesResponse
        {
            Absences = absences
                .OrderByDescending(a => a.StartDate)
                .Select(MapAbsence)
                .ToList(),
            AbsenceTypes = absenceTypes.ToOrderedLookup(),
            IllnessTypes = illnessTypes.ToAlphabeticalLookup()
        };
    }

    public async Task UpdateAbsencesAsync(Guid staffMemberId, StaffAbsencesUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Only an All-scope (HR) editor manages confidential rows and may set the flag.
        var isAllScope = await _accessService.CanAsync(staffMemberId, StaffArea.Absences, StaffAccess.EditAll,
            cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileAbsencesAsync(staffMemberId, model.Absences, isAllScope, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    // HR (All scope) and the staff member themselves see confidential absences; a line manager does not.
    private async Task<bool> CanSeeConfidentialAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var relationship = await _accessService.GetRelationshipAsync(staffMemberId, cancellationToken);

        if (relationship == StaffRelationship.Self)
        {
            return true;
        }

        return await _accessService.CanAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewAll | StaffAccess.EditAll, cancellationToken);
    }

    private async Task ReconcileAbsencesAsync(Guid staffMemberId, List<StaffAbsenceUpsertItem> incoming,
        bool isAllScope, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await _absenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // A Managed editor reconciles only the non-confidential set — confidential rows it can't
        // see are never candidates for deletion.
        var deletable = isAllScope ? existing : existing.Where(e => !e.IsConfidential);

        foreach (var row in deletable.Where(row => !keptIds.Contains(row.Id)))
        {
            // No soft-delete column on StaffAbsences — hard delete.
            await _absenceRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                // Guard: a Managed editor can never touch a confidential row, even by id.
                if (!isAllScope && entity.IsConfidential)
                {
                    continue;
                }

                ApplyAbsence(entity, item, isAllScope);
                await _absenceRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffAbsence { Id = SqlConvention.SequentialGuid(), StaffMemberId = staffMemberId };
                ApplyAbsence(created, item, isAllScope);
                await _absenceRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyAbsence(StaffAbsence entity, StaffAbsenceUpsertItem item, bool isAllScope)
    {
        entity.AbsenceTypeId = item.AbsenceTypeId;
        entity.IllnessTypeId = item.IllnessTypeId;
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        // Only HR (All scope) may mark an absence confidential; a line manager's rows stay open.
        entity.IsConfidential = isAllScope && item.IsConfidential;
        entity.Notes = item.Notes;
    }

    private static StaffAbsenceResponse MapAbsence(StaffAbsence a) => new()
    {
        Id = a.Id,
        AbsenceTypeId = a.AbsenceTypeId,
        IllnessTypeId = a.IllnessTypeId,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsConfidential = a.IsConfidential,
        Notes = a.Notes
    };
}
