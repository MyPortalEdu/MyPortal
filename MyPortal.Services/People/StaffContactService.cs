using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Staff-flavoured wrapper over <see cref="IPersonContactService"/>: enforces staff access
/// (relationship-scoped under <see cref="StaffArea.BasicDetails"/>), resolves the staff member to a
/// person, then delegates the email/phone mechanics. The reusable core lives on the person service.
/// </summary>
public class StaffContactService : BaseService, IStaffContactService
{
    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IPersonContactService _personContactService;

    public StaffContactService(IAuthorizationService authorizationService, ILogger<StaffContactService> logger,
        IStaffMemberAccessService accessService, IStaffMemberRepository staffMemberRepository,
        IPersonContactService personContactService)
        : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _personContactService = personContactService;
    }

    public async Task<StaffContactDetailsResponse> GetContactDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        // Contact methods live under the BasicDetails permission domain (see StaffMemberAccessService).
        await _accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        return await _personContactService.GetContactDetailsAsync(personId, cancellationToken);
    }

    public async Task UpdateContactDetailsAsync(Guid staffMemberId, StaffContactDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        await _personContactService.UpdateContactDetailsAsync(personId, model, cancellationToken);
    }

    private async Task<Guid> ResolvePersonIdAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        return staffMember.PersonId;
    }
}
