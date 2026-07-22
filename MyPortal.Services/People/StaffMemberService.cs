using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class StaffMemberService(
    IAuthorizationService authorizationService,
    ILogger<StaffMemberService> logger,
    IStaffMemberRepository staffMemberRepository,
    IPersonRepository personRepository,
    IStaffLineManagerRepository lineManagerRepository,
    IDateTimeProvider dateTimeProvider,
    IStaffMemberAccessService accessService,
    IPersonService personService,
    IPhotoService photoService,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffMemberService
{
    /// <summary>Minimum query length before <see cref="SearchPeopleAsync"/> hits the database —
    /// guards against an effectively unfiltered scan on a one-character term.</summary>
    private const int MinSearchLength = 2;

    public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        // Listing every staff member is inherently an All-scope read.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffBasicDetails, cancellationToken);

        return await staffMemberRepository.GetStaffMembersAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<StaffMemberHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken)
    {
        // Minimum-to-open: any view-basic-details scope covering this subject, else 403.
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var row = await staffMemberRepository.GetHeaderByIdAsync(id, cancellationToken);

        if (row == null)
        {
            // Only All-scope holders reach this branch; non-All viewers 403 above, so the
            // 404 here doesn't leak existence.
            throw new NotFoundException("Staff member not found.");
        }

        var relationship = await accessService.GetRelationshipAsync(id, cancellationToken);

        return new StaffMemberHeaderResponse
        {
            Id = row.Id,
            PersonId = row.PersonId,
            Code = row.Code,
            DisplayName = row.DisplayName,
            PreferredName = row.PreferredName,
            PhotoId = row.PhotoId,
            Status = DeriveStatus(row),
            Relationship = relationship
        };
    }

    // Lifecycle badge, highest-precedence first: a soft-deleted record is Archived; otherwise the
    // member is Active (has a current spell), Future (only future spells), Leaver (all spells ended),
    // or None (no employment on record). Derived here rather than in SQL so the enum values stay in C#.
    private static StaffStatus DeriveStatus(StaffMemberHeaderRow row)
    {
        if (row.IsDeleted) return StaffStatus.Archived;
        if (row.HasCurrentEmployment) return StaffStatus.Active;
        if (row.HasFutureEmployment) return StaffStatus.Future;
        if (row.HasAnyEmployment) return StaffStatus.Leaver;
        return StaffStatus.None;
    }

    public async Task<StaffBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var details = await staffMemberRepository.GetBasicDetailsByIdAsync(id, cancellationToken);

        if (details == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        return details;
    }

    public async Task UpdateBasicDetailsAsync(Guid id, StaffBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Only the staff code lives in this area on the StaffMember row; everything else
        // (LineManagerId, Bank*, NI, TRN, ...) belongs to other areas and stays untouched.
        staffMember.Code = model.Code;

        var bio = new PersonBasicBio(
            Title: model.Title,
            FirstName: model.FirstName,
            MiddleName: model.MiddleName,
            LastName: model.LastName,
            PreferredFirstName: model.PreferredFirstName,
            PreferredLastName: model.PreferredLastName,
            PhotoId: model.PhotoId,
            Gender: model.Gender,
            Dob: model.Dob,
            Deceased: model.Deceased);

        await unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
        {
            await EnsureCodeUniqueAsync(model.Code, excludeStaffMemberId: id, cancellationToken,
                ownedUow.Transaction);
            await personService.UpdateBasicBioAsync(staffMember.PersonId, bio, cancellationToken, ownedUow);
            await staffMemberRepository.UpdateAsync(staffMember, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task<StaffManagementResponse> GetManagementAsync(Guid id, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var management = await staffMemberRepository.GetManagementByIdAsync(id, cancellationToken)
                         ?? throw new NotFoundException("Staff member not found.");

        // Setting the manager is HR-owned (EditAll); the picker roster is only handed to those
        // who can actually use it, so a self-viewer never sees the full staff list.
        management.CanEdit = await accessService.CanAsync(id, StaffArea.BasicDetails,
            StaffAccess.EditAll, cancellationToken);

        if (management.CanEdit)
        {
            var options = await staffMemberRepository.GetStaffLookupAsync(cancellationToken);
            management.ManagerOptions = options.Where(o => o.Id != id).ToList();
        }

        var history = await lineManagerRepository.GetHistoryAsync(id, cancellationToken);
        management.History = history
            .Select(h => new StaffLineManagerHistoryResponse
            {
                Id = h.Id,
                LineManagerId = h.LineManagerId,
                LineManagerName = h.LineManagerName,
                LineManagerCode = h.LineManagerCode,
                StartDate = h.StartDate,
                EndDate = h.EndDate
            })
            .ToList();

        return management;
    }

    public async Task SetLineManagerAsync(Guid id, SetStaffLineManagerRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails, StaffAccess.EditAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken)
                          ?? throw new NotFoundException("Staff member not found.");

        if (model.LineManagerId is { } managerId)
        {
            if (managerId == id)
            {
                throw new ValidationException([new ValidationFailure(nameof(model.LineManagerId),
                    "A staff member cannot be their own line manager.")]);
            }

            var manager = await staffMemberRepository.GetByIdAsync(managerId, cancellationToken);

            if (manager == null || manager.IsDeleted)
            {
                throw new ValidationException([new ValidationFailure(nameof(model.LineManagerId),
                    "The selected line manager was not found.")]);
            }

            // Cycle guard: the proposed manager must not already sit below the subject in the chain.
            if (await staffMemberRepository.IsManagedByAsync(managerId, id, cancellationToken))
            {
                throw new ValidationException([new ValidationFailure(nameof(model.LineManagerId),
                    "That assignment would create a line-management cycle.")]);
            }
        }

        // The reporting line is date-ranged history: close today's row and open a new one rather
        // than overwriting, so "who managed them in 2023" stays answerable.
        var today = dateTimeProvider.UtcNow.Date;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var current = await lineManagerRepository.GetCurrentAsync(id, today, cancellationToken,
                uow.Transaction);

            if (current != null && current.LineManagerId == model.LineManagerId)
            {
                return;
            }

            if (current != null)
            {
                // A same-day correction replaces the row outright — otherwise we'd leave a
                // zero-length period behind.
                if (current.StartDate.Date == today)
                {
                    await lineManagerRepository.DeleteAsync(current.Id, cancellationToken, true, uow.Transaction);
                }
                else
                {
                    current.EndDate = today.AddDays(-1);
                    await lineManagerRepository.UpdateAsync(current, cancellationToken, uow.Transaction);
                }
            }

            if (model.LineManagerId is { } newManagerId)
            {
                await lineManagerRepository.InsertAsync(new StaffLineManager
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = id,
                    LineManagerId = newManagerId,
                    StartDate = today
                }, cancellationToken, uow.Transaction);
            }

            // Convenience copy — no longer authoritative (the procs read the history table).
            staffMember.LineManagerId = model.LineManagerId;
            await staffMemberRepository.UpdateAsync(staffMember, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken)
                          ?? throw new NotFoundException("Staff member not found.");

        await photoService.SetPhotoAsync(staffMember.PersonId, image, contentType, fileName, cancellationToken);
    }

    public async Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken)
                          ?? throw new NotFoundException("Staff member not found.");

        return await photoService.GetPhotoContentAsync(staffMember.PersonId, cancellationToken);
    }

    public async Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken)
                          ?? throw new NotFoundException("Staff member not found.");

        await photoService.DeletePhotoAsync(staffMember.PersonId, cancellationToken);
    }

    public async Task<Guid> CreateAsync(StaffBasicDetailsUpsertRequest model, CancellationToken cancellationToken)
    {
        // Create has no subject yet, so it can't be relationship-scoped — All only.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMemberId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            await EnsureCodeUniqueAsync(model.Code, excludeStaffMemberId: null, cancellationToken,
                ownedUow.Transaction);

            // Person row + its directory first; the staff row hangs off the new person, both in
            // one transaction.
            var bio = new PersonBasicBio(
                Title: model.Title,
                FirstName: model.FirstName,
                MiddleName: model.MiddleName,
                LastName: model.LastName,
                PreferredFirstName: model.PreferredFirstName,
                PreferredLastName: model.PreferredLastName,
                PhotoId: model.PhotoId,
                Gender: model.Gender,
                Dob: model.Dob,
                Deceased: model.Deceased);

            var personId = await personService.CreateAsync(bio, cancellationToken, ownedUow);

            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = personId,
                Code = model.Code
                // All other staff fields (LineManagerId, InductionStatusId, Bank*, NiNumber,
                // TRN, HasQts, etc.) stay at entity defaults — populated post-creation via
                // their area PUTs.
            };

            await staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

            return staffMemberId;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonMatchResponse>> SearchPeopleAsync(string? query,
        CancellationToken cancellationToken)
    {
        // Same gate as create: searching across every Person (students/contacts included) is an
        // HR-only capability, granted by the create scope.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        var trimmed = query?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < MinSearchLength)
        {
            return [];
        }

        // Escape LIKE wildcards in the user term so '%'/'_' are matched literally, then wrap as a
        // contains pattern. The SQL relies on the default escape char.
        var escaped = trimmed.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        return await staffMemberRepository.SearchPeopleForStaffCreateAsync($"%{escaped}%", cancellationToken);
    }

    public async Task<Guid> CreateForPersonAsync(StaffMemberCreateForPersonRequest model,
        CancellationToken cancellationToken)
    {
        // Attaching a staff role to any person is an All-scope action, same as a fresh create.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await validationService.ValidateAsync(model);

        var person = await personRepository.GetByIdAsync(model.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        // A person can only hold one staff role — block a duplicate StaffMember row. The FE
        // already disables already-staff matches; this is the server-side guard.
        var existing = await staffMemberRepository.GetStaffMemberIdByPersonIdAsync(model.PersonId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(model.PersonId), "This person is already a staff member.")]);
        }

        var staffMemberId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            await EnsureCodeUniqueAsync(model.Code, excludeStaffMemberId: null, cancellationToken,
                ownedUow.Transaction);

            // Only the StaffMember row is created — the Person (and its bio + directory) already
            // exists. All other staff fields stay at entity defaults, populated post-creation via
            // their area PUTs.
            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = model.PersonId,
                Code = model.Code
            };

            await staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

            return staffMemberId;
        }, cancellationToken);
    }

    // Staff codes must be unique across the school. The DB has no unique index, so guard here and
    // surface a friendly error. Pass excludeStaffMemberId on update so a member doesn't clash with
    // itself. A blank code (where the schema allows it) is left to the field-level validator.
    private async Task EnsureCodeUniqueAsync(string code, Guid? excludeStaffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        if (await staffMemberRepository.CodeExistsAsync(code, excludeStaffMemberId, cancellationToken, transaction))
        {
            throw new ValidationException(
                [new ValidationFailure("Code", $"Staff code '{code}' is already in use.")]);
        }
    }

    public async Task<bool> IsCodeAvailableAsync(string? code, Guid? excludeStaffMemberId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return true;
        }

        return !await staffMemberRepository.CodeExistsAsync(code.Trim(), excludeStaffMemberId,
            cancellationToken, null);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // HR-only action; All-scope.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Soft-delete the staff row only — the person may also be a contact/student.
        await staffMemberRepository.DeleteAsync(id, cancellationToken);
    }
}
