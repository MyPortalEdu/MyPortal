using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class StaffMemberService : BaseService, IStaffMemberService
{
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IStaffMemberAccessService _accessService;
    private readonly IPersonService _personService;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    /// <summary>Minimum query length before <see cref="SearchPeopleAsync"/> hits the database —
    /// guards against an effectively unfiltered scan on a one-character term.</summary>
    private const int MinSearchLength = 2;

    public StaffMemberService(IAuthorizationService authorizationService, ILogger<StaffMemberService> logger,
        IStaffMemberRepository staffMemberRepository, IPersonRepository personRepository,
        IStaffMemberAccessService accessService, IPersonService personService,
        IValidationService validationService, IUnitOfWorkFactory unitOfWorkFactory)
        : base(authorizationService, logger)
    {
        _staffMemberRepository = staffMemberRepository;
        _personRepository = personRepository;
        _accessService = accessService;
        _personService = personService;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        // Listing every staff member is inherently an All-scope read.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffBasicDetails, cancellationToken);

        return await _staffMemberRepository.GetStaffMembersAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<StaffMemberHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken)
    {
        // Minimum-to-open: any view-basic-details scope covering this subject, else 403.
        await _accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var row = await _staffMemberRepository.GetHeaderByIdAsync(id, cancellationToken);

        if (row == null)
        {
            // Only All-scope holders reach this branch; non-All viewers 403 above, so the
            // 404 here doesn't leak existence.
            throw new NotFoundException("Staff member not found.");
        }

        var relationship = await _accessService.GetRelationshipAsync(id, cancellationToken);

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
        await _accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var details = await _staffMemberRepository.GetBasicDetailsByIdAsync(id, cancellationToken);

        if (details == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        return details;
    }

    public async Task UpdateBasicDetailsAsync(Guid id, StaffBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(id, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(id, cancellationToken);

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

        await _unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
        {
            await _personService.UpdateBasicBioAsync(staffMember.PersonId, bio, cancellationToken, ownedUow);
            await _staffMemberRepository.UpdateAsync(staffMember, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task<Guid> CreateAsync(StaffBasicDetailsUpsertRequest model, CancellationToken cancellationToken)
    {
        // Create has no subject yet, so it can't be relationship-scoped — All only.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMemberId = SqlConvention.SequentialGuid();

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
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

            var personId = await _personService.CreateAsync(bio, cancellationToken, ownedUow);

            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = personId,
                Code = model.Code
                // All other staff fields (LineManagerId, InductionStatusId, Bank*, NiNumber,
                // TRN, HasQts, etc.) stay at entity defaults — populated post-creation via
                // their area PUTs.
            };

            await _staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

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

        return await _staffMemberRepository.SearchPeopleForStaffCreateAsync($"%{escaped}%", cancellationToken);
    }

    public async Task<Guid> CreateForPersonAsync(StaffMemberCreateForPersonRequest model,
        CancellationToken cancellationToken)
    {
        // Attaching a staff role to any person is an All-scope action, same as a fresh create.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await _validationService.ValidateAsync(model);

        var person = await _personRepository.GetByIdAsync(model.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        // A person can only hold one staff role — block a duplicate StaffMember row. The FE
        // already disables already-staff matches; this is the server-side guard.
        var existing = await _staffMemberRepository.GetStaffMemberIdByPersonIdAsync(model.PersonId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(model.PersonId), "This person is already a staff member.")]);
        }

        var staffMemberId = SqlConvention.SequentialGuid();

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Only the StaffMember row is created — the Person (and its bio + directory) already
            // exists. All other staff fields stay at entity defaults, populated post-creation via
            // their area PUTs.
            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = model.PersonId,
                Code = model.Code
            };

            await _staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

            return staffMemberId;
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // HR-only action; All-scope.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Soft-delete the staff row only — the person may also be a contact/student.
        await _staffMemberRepository.DeleteAsync(id, cancellationToken);
    }
}
