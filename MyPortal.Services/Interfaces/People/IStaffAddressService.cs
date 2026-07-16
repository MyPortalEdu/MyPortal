using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Addresses section of the staff Contact Details area. Addresses are shared entities, so writes
/// are granular (link/create, update, unlink) rather than a whole-collection replace, and editing
/// a shared address is governed by <see cref="AddressEditMode"/>. Gated as part of
/// <see cref="StaffArea.BasicDetails"/>.
/// </summary>
public interface IStaffAddressService
{
    Task<AddressListResponse> GetAddressesAsync(Guid staffMemberId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(Guid staffMemberId, string? query,
        CancellationToken cancellationToken);

    /// <summary>Link an existing address or create a new one. Returns the new link (AddressPerson) id.</summary>
    Task<Guid> AddAddressAsync(Guid staffMemberId, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken);

    Task UpdateAddressAsync(Guid staffMemberId, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken);

    Task RemoveAddressAsync(Guid staffMemberId, Guid addressPersonId, CancellationToken cancellationToken);
}
