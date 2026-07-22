namespace MyPortal.Contracts.Models.People;

public class AddressMatchResponse
{
    public Guid AddressId { get; set; }

    public string? BuildingNumber { get; set; }
    public string? BuildingName { get; set; }
    public string? Apartment { get; set; }
    public string Street { get; set; } = null!;
    public string? District { get; set; }
    public string Town { get; set; } = null!;
    public string County { get; set; } = null!;
    public string Postcode { get; set; } = null!;
    public string Country { get; set; } = null!;

    public int LinkedPersonCount { get; set; }
}
