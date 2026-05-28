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
    private readonly IPersonService _personService;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffMemberService(IAuthorizationService authorizationService, ILogger<StaffMemberService> logger,
        IStaffMemberRepository staffMemberRepository, IPersonService personService,
        IValidationService validationService, IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _staffMemberRepository = staffMemberRepository;
        _personService = personService;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        // Listing every staff member is inherently an All-scope read; consumers such as the
        // school-details head-teacher picker must hold this. (A future Own/Managed-scoped staff
        // list would filter to the subjects the viewer can see — out of scope here.)
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffBasicDetails, cancellationToken);

        return await _staffMemberRepository.GetStaffMembersAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<StaffMemberDetailsResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        // INTERIM: flat All-scope gate until IStaffMemberAccessService lands (steps 4–5 of
        // docs/staff-profile-access.md), at which point this becomes a relationship-aware
        // RequireAsync(id, BasicDetails, View). All-scope is a strict subset of the final
        // behaviour, so this never over-exposes — it's just temporarily stricter for self/managed.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffBasicDetails, cancellationToken);

        var details = await _staffMemberRepository.GetDetailsByIdAsync(id, cancellationToken);

        if (details == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        return details;
    }

    public async Task<Guid> CreateAsync(StaffMemberUpsertRequest model, CancellationToken cancellationToken)
    {
        // Creating a new staff member isn't relationship-scoped (there's no subject yet), so this
        // stays an All-scope action permanently.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMemberId = SqlConvention.SequentialGuid();

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Person + directory created first; the staff member hangs off the new person.
            // Passing ownedUow keeps both rows in one transaction (and tells PersonService
            // the staff gate above already covers this).
            var personId = await _personService.CreateAsync(model.Person, cancellationToken, ownedUow);

            var staffMember = new StaffMember
            {
                Id = staffMemberId,
                PersonId = personId
            };

            ApplyStaffFields(staffMember, model);
            staffMember.IsDeleted = false;

            await _staffMemberRepository.InsertAsync(staffMember, cancellationToken, ownedUow.Transaction);

            return staffMemberId;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, StaffMemberUpsertRequest model, CancellationToken cancellationToken)
    {
        // INTERIM: flat All-scope gate until the resolver lands (becomes RequireAsync(id,
        // BasicDetails, Edit) + the relevant section gates). See GetDetailsAsync note.
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
        // INTERIM: flat All-scope gate until the resolver lands. See GetDetailsAsync note.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffBasicDetails, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(id, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        // Soft-delete the staff member only. The underlying Person may also be a
        // contact/student and is left intact; reinstating a leaver is a future slice.
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
