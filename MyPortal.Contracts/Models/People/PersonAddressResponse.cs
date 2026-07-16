namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One address linked to a person. Carries the link id (<see cref="AddressPersonId"/>, the key for
/// update/unlink), the shared <see cref="AddressId"/>, the link's type/main flag, the address
/// fields, and how many people share this address.
/// </summary>
public class PersonAddressResponse
{
    public Guid AddressPersonId { get; set; }
    public Guid AddressId { get; set; }
    public Guid TypeId { get; set; }
    public bool IsMain { get; set; }

    public string? BuildingNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? Apartment { get; set; }
    public string Street { get; set; } = null!;
    public string? District { get; set; }
    public string Town { get; set; } = null!;
    public string County { get; set; } = null!;
    public string Postcode { get; set; } = null!;
    public string Country { get; set; } = null!;

    /// <summary>How many people are linked to this address, including this one. &gt; 1 means shared.</summary>
    public int SharedCount { get; set; }
}
