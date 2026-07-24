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
public class StaffAbsenceService(
    IAuthorizationService authorizationService,
    ILogger<StaffAbsenceService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IStaffAbsenceRepository absenceRepository,
    IStaffAbsenceTypeRepository absenceTypeRepository,
    IStaffIllnessTypeRepository illnessTypeRepository,
    IStaffAbsenceCertificateRepository certificateRepository,
    IStaffAbsencePayRateRepository payRateRepository,
    IStaffAbsencePayrollReasonRepository payrollReasonRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffAbsenceService
{
    public async Task<StaffAbsencesResponse> GetAbsencesAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var canSeeConfidential = await CanSeeConfidentialAsync(staffMemberId, cancellationToken);

        var absences = await absenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);

        if (!canSeeConfidential)
        {
            absences = absences.Where(a => !a.IsConfidential);
        }

        var absenceList = absences.ToList();

        var certificates =
            await certificateRepository.GetByAbsenceIdsAsync(absenceList.Select(a => a.Id), cancellationToken);
        var certificatesByAbsence = certificates
            .GroupBy(c => c.StaffAbsenceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var absenceTypes = await absenceTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var illnessTypes = await illnessTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        // The statutory pay treatment is HR's to set — a line manager doesn't get the option lists.
        var canEditPayroll = await accessService.CanAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.EditAll, cancellationToken);

        var response = new StaffAbsencesResponse
        {
            Absences = absenceList
                .OrderByDescending(a => a.StartDate)
                .Select(a => MapAbsence(a, certificatesByAbsence))
                .ToList(),
            AbsenceTypes = absenceTypes.ToOrderedLookup(),
            IllnessTypes = illnessTypes.ToAlphabeticalLookup(),
            CanEditPayroll = canEditPayroll
        };

        if (canEditPayroll)
        {
            var payRates = await payRateRepository.GetListAsync(cancellationToken: cancellationToken);
            var payrollReasons = await payrollReasonRepository.GetListAsync(cancellationToken: cancellationToken);
            response.PayRates = payRates.ToOrderedLookup();
            response.PayrollReasons = payrollReasons.ToOrderedLookup();
        }

        return response;
    }

    public async Task UpdateAbsencesAsync(Guid staffMemberId, StaffAbsencesUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Only an All-scope (HR) editor manages confidential rows and may set the flag.
        var isAllScope = await accessService.CanAsync(staffMemberId, StaffArea.Absences, StaffAccess.EditAll,
            cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileAbsencesAsync(staffMemberId, model.Absences, isAllScope, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    // HR (All scope) and the staff member themselves see confidential absences; a line manager does not.
    private async Task<bool> CanSeeConfidentialAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var relationship = await accessService.GetRelationshipAsync(staffMemberId, cancellationToken);

        if (relationship == StaffRelationship.Self)
        {
            return true;
        }

        return await accessService.CanAsync(staffMemberId, StaffArea.Absences,
            StaffAccess.ViewAll | StaffAccess.EditAll, cancellationToken);
    }

    private async Task ReconcileAbsencesAsync(Guid staffMemberId, List<StaffAbsenceUpsertItem> incoming,
        bool isAllScope, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await absenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // A Managed editor reconciles only the non-confidential set — confidential rows it can't
        // see are never candidates for deletion.
        var deletable = isAllScope ? existing : existing.Where(e => !e.IsConfidential);

        var droppedIds = deletable.Where(row => !keptIds.Contains(row.Id)).Select(row => row.Id).ToList();

        if (droppedIds.Count > 0)
        {
            // Certificates hang off the absence, so they go first — nothing is left orphaned.
            await DeleteCertificatesForAbsencesAsync(droppedIds, transaction, cancellationToken);

            foreach (var id in droppedIds)
            {
                // No soft-delete column on StaffAbsences — hard delete.
                await absenceRepository.DeleteAsync(id, cancellationToken, false, transaction);
            }
        }

        foreach (var item in incoming)
        {
            Guid absenceId;

            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                // Guard: a Managed editor can never touch a confidential row, even by id.
                if (!isAllScope && entity.IsConfidential)
                {
                    continue;
                }

                ApplyAbsence(entity, item, isAllScope);
                await absenceRepository.UpdateAsync(entity, cancellationToken, transaction);
                absenceId = entity.Id;
            }
            else
            {
                var created = new StaffAbsence { Id = SqlConvention.SequentialGuid(), StaffMemberId = staffMemberId };
                ApplyAbsence(created, item, isAllScope);
                await absenceRepository.InsertAsync(created, cancellationToken, transaction);
                absenceId = created.Id;
            }

            await ReconcileCertificatesAsync(absenceId, item.Certificates, transaction, cancellationToken);
        }
    }

    private async Task ReconcileCertificatesAsync(Guid absenceId, List<StaffAbsenceCertificateUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await certificateRepository.GetByAbsenceIdsAsync(new[] { absenceId }, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(c => c.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await certificateRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyCertificate(entity, item);
                await certificateRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffAbsenceCertificate
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffAbsenceId = absenceId
                };
                ApplyCertificate(created, item);
                await certificateRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private async Task DeleteCertificatesForAbsencesAsync(IEnumerable<Guid> absenceIds,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var orphaned = await certificateRepository.GetByAbsenceIdsAsync(absenceIds, cancellationToken, transaction);

        foreach (var certificate in orphaned)
        {
            await certificateRepository.DeleteAsync(certificate.Id, cancellationToken, false, transaction);
        }
    }

    private static void ApplyCertificate(StaffAbsenceCertificate entity, StaffAbsenceCertificateUpsertItem item)
    {
        entity.DateReceived = item.DateReceived;
        entity.DateSigned = item.DateSigned;
        entity.IsSelfCertified = item.IsSelfCertified;
        entity.IsReturnToWork = item.IsReturnToWork;
        // A self-certified note has no external signatory.
        entity.SignedBy = item.IsSelfCertified ? null : item.SignedBy;
        entity.Notes = item.Notes;
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

        // Duration and the industrial-injury flag are part of managing the absence, so a line
        // manager may set them.
        entity.WorkingDaysLost = item.WorkingDaysLost;
        entity.HoursLost = item.HoursLost;
        entity.IsIndustrialInjury = item.IsIndustrialInjury;

        // The statutory pay treatment is payroll's business — All scope (HR) only. A Managed
        // editor's payload can never change it, and never clears what HR has already set.
        if (isAllScope)
        {
            entity.AuthorisedPayRateId = item.AuthorisedPayRateId;
            entity.PayrollReasonId = item.PayrollReasonId;
            entity.SspExcluded = item.SspExcluded;
        }
    }

    private static StaffAbsenceResponse MapAbsence(StaffAbsence a,
        IReadOnlyDictionary<Guid, List<StaffAbsenceCertificate>> certificatesByAbsence) => new()
    {
        Id = a.Id,
        AbsenceTypeId = a.AbsenceTypeId,
        IllnessTypeId = a.IllnessTypeId,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsConfidential = a.IsConfidential,
        Notes = a.Notes,
        AuthorisedPayRateId = a.AuthorisedPayRateId,
        PayrollReasonId = a.PayrollReasonId,
        SspExcluded = a.SspExcluded,
        WorkingDaysLost = a.WorkingDaysLost,
        HoursLost = a.HoursLost,
        IsIndustrialInjury = a.IsIndustrialInjury,
        Certificates = (certificatesByAbsence.TryGetValue(a.Id, out var rows) ? rows : [])
            .OrderByDescending(c => c.DateReceived)
            .Select(MapCertificate)
            .ToList()
    };

    private static StaffAbsenceCertificateResponse MapCertificate(StaffAbsenceCertificate c) => new()
    {
        Id = c.Id,
        DateReceived = c.DateReceived,
        DateSigned = c.DateSigned,
        IsSelfCertified = c.IsSelfCertified,
        IsReturnToWork = c.IsReturnToWork,
        SignedBy = c.SignedBy,
        Notes = c.Notes
    };
}
