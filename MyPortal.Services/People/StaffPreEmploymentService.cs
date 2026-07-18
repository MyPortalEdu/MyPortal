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
/// Owns the Pre-Employment Checks (Single Central Record) area: a 1:1 summary record of SCR
/// "checked on" flags plus the DBS, right-to-work, reference and overseas-check lists. Gating is
/// enforced under <see cref="StaffArea.PreEmploymentChecks"/> — safeguarding/HR data, All-scope
/// view and edit only. The save is a whole-area replace, each list reconciled by id.
/// </summary>
public class StaffPreEmploymentService(
    IAuthorizationService authorizationService,
    ILogger<StaffPreEmploymentService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IStaffPreEmploymentChecksRepository checksRepository,
    IDbsCheckRepository dbsCheckRepository,
    IDbsCheckTypeRepository dbsCheckTypeRepository,
    IRightToWorkCheckRepository rightToWorkRepository,
    IRightToWorkDocumentTypeRepository rightToWorkDocumentTypeRepository,
    IStaffReferenceRepository referenceRepository,
    IReferenceTypeRepository referenceTypeRepository,
    IReferenceStatusRepository referenceStatusRepository,
    IStaffOverseasCheckRepository overseasRepository,
    INationalityRepository nationalityRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffPreEmploymentService
{
    public async Task<StaffPreEmploymentChecksResponse> GetPreEmploymentChecksAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        // Safeguarding/HR data — All-scope view only (no self / line-manager scope).
        await accessService.RequireAsync(staffMemberId, StaffArea.PreEmploymentChecks, StaffAccess.ViewAll,
            cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var record = await checksRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var dbsChecks = await dbsCheckRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var rightToWorkChecks = await rightToWorkRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var references = await referenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var overseasChecks = await overseasRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);

        var dbsCheckTypes = await dbsCheckTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var rightToWorkDocumentTypes =
            await rightToWorkDocumentTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var referenceTypes = await referenceTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var referenceStatuses = await referenceStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var nationalities = await nationalityRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffPreEmploymentChecksResponse
        {
            IdentityCheckedDate = record?.IdentityCheckedDate,
            ProhibitionFromTeachingCheckedDate = record?.ProhibitionFromTeachingCheckedDate,
            ProhibitionFromManagementCheckedDate = record?.ProhibitionFromManagementCheckedDate,
            ChildcareDisqualificationCheckedDate = record?.ChildcareDisqualificationCheckedDate,
            MedicalFitnessCheckedDate = record?.MedicalFitnessCheckedDate,
            QualificationsVerifiedDate = record?.QualificationsVerifiedDate,
            Notes = record?.Notes,
            DbsChecks = dbsChecks.OrderByDescending(d => d.IssueDate).Select(MapDbs).ToList(),
            RightToWorkChecks = rightToWorkChecks.OrderByDescending(r => r.CheckDate).Select(MapRightToWork).ToList(),
            References = references.OrderBy(r => r.RefereeName).Select(MapReference).ToList(),
            OverseasChecks = overseasChecks.OrderByDescending(o => o.CheckedDate).Select(MapOverseas).ToList(),
            DbsCheckTypes = dbsCheckTypes.ToAlphabeticalLookup(),
            RightToWorkDocumentTypes = rightToWorkDocumentTypes.ToAlphabeticalLookup(),
            ReferenceTypes = referenceTypes.ToOrderedLookup(),
            ReferenceStatuses = referenceStatuses.ToOrderedLookup(),
            Countries = nationalities.ToAlphabeticalLookup()
        };
    }

    public async Task UpdatePreEmploymentChecksAsync(Guid staffMemberId,
        StaffPreEmploymentChecksUpsertRequest model, CancellationToken cancellationToken)
    {
        // Safeguarding/HR data — All-scope edit only.
        await accessService.RequireAsync(staffMemberId, StaffArea.PreEmploymentChecks, StaffAccess.EditAll,
            cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // The verifier on new right-to-work rows is the current user, but only where they are
        // themselves a staff member (the FK targets StaffMembers); otherwise it stays null.
        var personId = AuthorizationService.GetCurrentUserPersonId();
        var verifierId = personId.HasValue
            ? await staffMemberRepository.GetStaffMemberIdByPersonIdAsync(personId.Value, cancellationToken)
            : null;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await UpsertChecksRecordAsync(staffMemberId, model, uow.Transaction, cancellationToken);
            await ReconcileDbsChecksAsync(staffMemberId, model.DbsChecks, uow.Transaction, cancellationToken);
            await ReconcileRightToWorkAsync(staffMemberId, model.RightToWorkChecks, verifierId, uow.Transaction,
                cancellationToken);
            await ReconcileReferencesAsync(staffMemberId, model.References, uow.Transaction, cancellationToken);
            await ReconcileOverseasAsync(staffMemberId, model.OverseasChecks, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task UpsertChecksRecordAsync(Guid staffMemberId, StaffPreEmploymentChecksUpsertRequest model,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = await checksRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction);

        if (existing != null)
        {
            ApplyChecksRecord(existing, model);
            await checksRepository.UpdateAsync(existing, cancellationToken, transaction);
        }
        else
        {
            var record = new StaffPreEmploymentChecks
            {
                Id = SqlConvention.SequentialGuid(),
                StaffMemberId = staffMemberId
            };
            ApplyChecksRecord(record, model);
            await checksRepository.InsertAsync(record, cancellationToken, transaction);
        }
    }

    private static void ApplyChecksRecord(StaffPreEmploymentChecks entity, StaffPreEmploymentChecksUpsertRequest model)
    {
        entity.IdentityCheckedDate = model.IdentityCheckedDate;
        entity.ProhibitionFromTeachingCheckedDate = model.ProhibitionFromTeachingCheckedDate;
        entity.ProhibitionFromManagementCheckedDate = model.ProhibitionFromManagementCheckedDate;
        entity.ChildcareDisqualificationCheckedDate = model.ChildcareDisqualificationCheckedDate;
        entity.MedicalFitnessCheckedDate = model.MedicalFitnessCheckedDate;
        entity.QualificationsVerifiedDate = model.QualificationsVerifiedDate;
        entity.Notes = model.Notes;
    }

    private async Task ReconcileDbsChecksAsync(Guid staffMemberId, List<DbsCheckUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await dbsCheckRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await dbsCheckRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyDbs(entity, item);
                await dbsCheckRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new DbsCheck { Id = SqlConvention.SequentialGuid(), StaffMemberId = staffMemberId };
                ApplyDbs(created, item);
                await dbsCheckRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyDbs(DbsCheck entity, DbsCheckUpsertItem item)
    {
        entity.DbsCheckTypeId = item.DbsCheckTypeId;
        entity.CertificateNumber = item.CertificateNumber;
        entity.IssueDate = item.IssueDate;
        entity.ExpiryDate = item.ExpiryDate;
        entity.UpdateServiceEnrolled = item.UpdateServiceEnrolled;
        entity.LastUpdateServiceCheck = item.LastUpdateServiceCheck;
        entity.Notes = item.Notes;
    }

    private async Task ReconcileRightToWorkAsync(Guid staffMemberId, List<RightToWorkCheckUpsertItem> incoming,
        Guid? verifierId, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await rightToWorkRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await rightToWorkRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyRightToWork(entity, item);
                await rightToWorkRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new RightToWorkCheck
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId,
                    VerifiedById = verifierId
                };
                ApplyRightToWork(created, item);
                await rightToWorkRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyRightToWork(RightToWorkCheck entity, RightToWorkCheckUpsertItem item)
    {
        entity.DocumentTypeId = item.DocumentTypeId;
        entity.DocumentNumber = item.DocumentNumber;
        entity.CheckDate = item.CheckDate;
        entity.DocumentExpiryDate = item.DocumentExpiryDate;
        entity.FollowUpDate = item.FollowUpDate;
        entity.Notes = item.Notes;
    }

    private async Task ReconcileReferencesAsync(Guid staffMemberId, List<StaffReferenceUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await referenceRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await referenceRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyReference(entity, item);
                await referenceRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffReference { Id = SqlConvention.SequentialGuid(), StaffMemberId = staffMemberId };
                ApplyReference(created, item);
                await referenceRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyReference(StaffReference entity, StaffReferenceUpsertItem item)
    {
        entity.ReferenceTypeId = item.ReferenceTypeId;
        entity.ReferenceStatusId = item.ReferenceStatusId;
        entity.RefereeName = item.RefereeName;
        entity.RefereeOrganisation = item.RefereeOrganisation;
        entity.RefereeEmail = item.RefereeEmail;
        entity.RequestedDate = item.RequestedDate;
        entity.ReceivedDate = item.ReceivedDate;
        entity.Notes = item.Notes;
    }

    private async Task ReconcileOverseasAsync(Guid staffMemberId, List<StaffOverseasCheckUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await overseasRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await overseasRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyOverseas(entity, item);
                await overseasRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffOverseasCheck
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId
                };
                ApplyOverseas(created, item);
                await overseasRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyOverseas(StaffOverseasCheck entity, StaffOverseasCheckUpsertItem item)
    {
        entity.NationalityId = item.NationalityId;
        entity.CheckedDate = item.CheckedDate;
        entity.IsClear = item.IsClear;
        entity.Notes = item.Notes;
    }

    private static DbsCheckResponse MapDbs(DbsCheck c) => new()
    {
        Id = c.Id,
        DbsCheckTypeId = c.DbsCheckTypeId,
        CertificateNumber = c.CertificateNumber,
        IssueDate = c.IssueDate,
        ExpiryDate = c.ExpiryDate,
        UpdateServiceEnrolled = c.UpdateServiceEnrolled,
        LastUpdateServiceCheck = c.LastUpdateServiceCheck,
        Notes = c.Notes
    };

    private static RightToWorkCheckResponse MapRightToWork(RightToWorkCheck c) => new()
    {
        Id = c.Id,
        DocumentTypeId = c.DocumentTypeId,
        DocumentNumber = c.DocumentNumber,
        CheckDate = c.CheckDate,
        DocumentExpiryDate = c.DocumentExpiryDate,
        FollowUpDate = c.FollowUpDate,
        Notes = c.Notes
    };

    private static StaffReferenceResponse MapReference(StaffReference r) => new()
    {
        Id = r.Id,
        ReferenceTypeId = r.ReferenceTypeId,
        ReferenceStatusId = r.ReferenceStatusId,
        RefereeName = r.RefereeName,
        RefereeOrganisation = r.RefereeOrganisation,
        RefereeEmail = r.RefereeEmail,
        RequestedDate = r.RequestedDate,
        ReceivedDate = r.ReceivedDate,
        Notes = r.Notes
    };

    private static StaffOverseasCheckResponse MapOverseas(StaffOverseasCheck o) => new()
    {
        Id = o.Id,
        NationalityId = o.NationalityId,
        CheckedDate = o.CheckedDate,
        IsClear = o.IsClear,
        Notes = o.Notes
    };
}
