namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// Contact profile header — identity + photo only. A Contact has no lifecycle/status, no admission
/// number and no registration, so there is no badge here (unlike the student header). Per-section
/// bodies fetch separately, each gated server-side; the FE composes the sidebar from its own
/// permission claim (contact access is flat/role-based, so no viewer relationship is returned).
/// </summary>
public class ContactHeaderResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }

    /// <summary>"Title First [Middle] Last", composed from the legal name.</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>Preferred first name (or null).</summary>
    public string? PreferredName { get; set; }

    public Guid? PhotoId { get; set; }
}
