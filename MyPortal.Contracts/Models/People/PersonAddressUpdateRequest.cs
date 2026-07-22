namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Update a person's address link: its type/main flag and the address fields. <see cref="Mode"/>
/// only matters when the address is shared — it decides whether the edit applies to everyone
/// (FixInPlace) or forks a new address for this person (Moved).
/// </summary>
public class PersonAddressUpdateRequest
{
    public Guid TypeId { get; set; }
    public bool IsMain { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AddressEditMode Mode { get; set; }

    public string? BuildingNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? Apartment { get; set; }
    public string Street { get; set; } = null!;
    public string? District { get; set; }
    public string Town { get; set; } = null!;
    public string County { get; set; } = null!;
    public string Postcode { get; set; } = null!;
    public string Country { get; set; } = null!;
}
