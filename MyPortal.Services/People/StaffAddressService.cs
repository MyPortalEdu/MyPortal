using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Staff-flavoured wrapper over <see cref="IPersonAddressService"/>: enforces staff access
/// (relationship-scoped under <see cref="StaffArea.BasicDetails"/>), resolves the staff member to a
/// person, then delegates the shared-address mechanics. The reusable core lives on the person service.
/// </summary>
public class StaffAddressService(
    IAuthorizationService authorizationService,
    ILogger<StaffAddressService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IPersonAddressService personAddressService)
    : BaseService(authorizationService, logger), IStaffAddressService
{
    public async Task<AddressListResponse> GetAddressesAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        return await personAddressService.GetAddressesAsync(personId, cancellationToken);
    }

    public async Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(Guid staffMemberId, string? query,
        CancellationToken cancellationToken)
    {
        // Searching to link is part of editing the area.
        await accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        return await personAddressService.SearchAddressesAsync(query, cancellationToken);
    }

    public async Task<Guid> AddAddressAsync(Guid staffMemberId, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        return await personAddressService.AddAddressAsync(personId, model, cancellationToken);
    }

    public async Task UpdateAddressAsync(Guid staffMemberId, Guid addressPersonId,
        PersonAddressUpdateRequest model, CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        await personAddressService.UpdateAddressAsync(personId, addressPersonId, model, cancellationToken);
    }

    public async Task RemoveAddressAsync(Guid staffMemberId, Guid addressPersonId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.BasicDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        var personId = await ResolvePersonIdAsync(staffMemberId, cancellationToken);

        await personAddressService.RemoveAddressAsync(personId, addressPersonId, cancellationToken);
    }

    private async Task<Guid> ResolvePersonIdAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        return staffMember.PersonId;
    }
}
