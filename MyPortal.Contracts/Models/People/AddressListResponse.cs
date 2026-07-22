namespace MyPortal.Contracts.Models.People;

public class AddressListResponse
{
    public List<PersonAddressResponse> Addresses { get; set; } = [];
    public List<LookupResponse> AddressTypes { get; set; } = [];
}
