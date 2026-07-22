namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Add an address to a person. Either link an existing address (<see cref="ExistingAddressId"/>
/// set, from the search) or create a new one (address fields supplied, id null). The server also
/// dedupes new addresses against an exact match (postcode + building + street) to avoid duplicate
/// rows.
/// </summary>
public class PersonAddressUpsertRequest
{
    /// <summary>When set, link this existing address — the address fields below are ignored.</summary>
    public Guid? ExistingAddressId { get; set; }

    public Guid TypeId { get; set; }
    public bool IsMain { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Used only when creating a new address (ExistingAddressId is null).
    public string? BuildingNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? Apartment { get; set; }
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? Town { get; set; }
    public string? County { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
}
