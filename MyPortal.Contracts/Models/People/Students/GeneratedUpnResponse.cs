namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// A freshly generated (permanent) UPN suggestion for the registration editor. Not persisted or
/// reserved — the caller writes it into the UPN field and it is saved with the registration.
/// </summary>
public class GeneratedUpnResponse
{
    public string Upn { get; set; } = null!;
}
