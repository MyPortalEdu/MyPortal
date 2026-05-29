using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
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
    private readonly IStaffMemberAccessService _accessService;
    private readonly IPersonService _personService;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffMemberService(IAuthorizationService authorizationService, ILogger<StaffMemberService> logger,
        IStaffMemberRepository staffMemberRepository, IStaffMemberAccessService accessService,
        IPersonService personService, IValidationService validationService, IUnitOfWorkFactory unitOfWorkFactory)
        : base(authorizationService, logger)
    {
        _staffMemberRepository = staffMemberRepository;
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
            Status = row.IsDeleted ? StaffStatus.Inactive : StaffStatus.Active,
            Relationship = relationship
        };
    }

    public async Task<Guid> CreateAsync(StaffMemberUpsertRequest model, CancellationToken cancellationToken)
    {
        // Create has no subject yet, so it can't be relationship-scoped — All only.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMemberId = SqlConvention.SequentialGuid();

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Person row + its directory first; the staff row hangs off the new person, both in
            // one transaction.
            var personId = await _personService.CreateAsync(model.Person, cancellationToken, ownedUow);

            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = personId
            };

            ApplyStaffFields(staffMember, model);

            await _staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

            return staffMemberId;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, StaffMemberUpsertRequest model, CancellationToken cancellationToken)
    {
        // INTERIM: monolithic update; will be replaced by per-section PUTs once those land.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        ApplyStaffFields(staffMember, model);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
        {
            await _personService.UpdateAsync(staffMember.PersonId, model.Person, cancellationToken, ownedUow);

            await _staffMemberRepository.UpdateAsync(staffMember, cancellationToken, ownedUow.Transaction);
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

    private static void ApplyStaffFields(StaffMember staffMember, StaffMemberUpsertRequest model)
    {
        staffMember.LineManagerId = model.LineManagerId;
        staffMember.InductionStatusId = model.InductionStatusId;
        staffMember.Code = model.Code;
        staffMember.BankName = model.BankName;
        staffMember.BankAccount = model.BankAccount;
        staffMember.BankSortCode = model.BankSortCode;
        staffMember.NiNumber = model.NiNumber;
        staffMember.TeacherReferenceNumber = model.TeacherReferenceNumber;
        staffMember.Qualifications = model.Qualifications;
        staffMember.IsTeachingStaff = model.IsTeachingStaff;
        staffMember.HasQts = model.HasQts;
        staffMember.QtsAwardedDate = model.QtsAwardedDate;
        staffMember.InductionStartDate = model.InductionStartDate;
        staffMember.InductionCompletedDate = model.InductionCompletedDate;
        staffMember.HasDisability = model.HasDisability;
        staffMember.DisabilityDetails = model.DisabilityDetails;
        staffMember.PpaPeriodsPerWeek = model.PpaPeriodsPerWeek;
    }
}
