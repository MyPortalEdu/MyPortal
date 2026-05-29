namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Staff profile header. Per-section bodies fetch separately, each gated server-side; the FE
/// composes the sidebar from <see cref="Relationship"/> + its own permission claim. Roles, line
/// manager, start date, FTE, site, and the Leaver status state land with their section slices.
/// </summary>
public class StaffMemberHeaderResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Code { get; set; } = null!;

    /// <summary>"Title First [Middle] Last", composed from the legal name.</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>Preferred first name (or null).</summary>
    public string? PreferredName { get; set; }

    public Guid? PhotoId { get; set; }
    public StaffStatus Status { get; set; }
    public StaffRelationship Relationship { get; set; }
}
