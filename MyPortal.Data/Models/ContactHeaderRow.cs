namespace MyPortal.Data.Models;

/// <summary>
/// Raw row from <c>usp_contact_get_header_by_id</c>. Identity + photo only — a contact has no
/// lifecycle/status, so there are no dates or IsDeleted to map into a badge.
/// </summary>
public sealed class ContactHeaderRow
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PreferredName { get; set; }
    public Guid? PhotoId { get; set; }
}
