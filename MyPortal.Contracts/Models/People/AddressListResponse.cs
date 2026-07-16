namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the addresses section of the Contact Details area: a person's linked
/// addresses plus the active type options for the editor.
/// </summary>
public class AddressListResponse
{
    public List<PersonAddressResponse> Addresses { get; set; } = [];
    public List<LookupResponse> AddressTypes { get; set; } = [];
}
