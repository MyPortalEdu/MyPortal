using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Person-level address mechanics keyed by <c>personId</c>. Addresses are shared entities, so writes
/// are granular (link/create, update, unlink) and editing a shared address is governed by
/// <see cref="AddressEditMode"/>. Subtype-agnostic and <em>unauthorized</em>: callers (e.g.
/// <see cref="IStaffAddressService"/>) resolve the person and enforce access first. This is the
/// shared core that staff / students / contacts delegate to.
/// </summary>
public interface IPersonAddressService
{
    Task<AddressListResponse> GetAddressesAsync(Guid personId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken);

    /// <summary>Link an existing address or create a new one. Returns the new link (AddressPerson) id.</summary>
    Task<Guid> AddAddressAsync(Guid personId, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken);

    Task UpdateAddressAsync(Guid personId, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken);

    Task RemoveAddressAsync(Guid personId, Guid addressPersonId, CancellationToken cancellationToken);
}
