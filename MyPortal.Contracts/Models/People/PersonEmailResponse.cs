namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single email address owned by a person. <see cref="TypeId"/> resolves against the
/// EmailTypes list on <see cref="StaffContactDetailsResponse"/>.
/// </summary>
public class PersonEmailResponse
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public string Address { get; set; } = null!;
    public bool IsMain { get; set; }
    public string? Notes { get; set; }
}
