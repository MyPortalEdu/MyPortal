namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Lightweight staff row for "pick a staff member" UI. <see cref="Id"/> is the StaffMember id
/// (the key for the staff profile / CRUD endpoints); <see cref="PersonId"/> is the underlying
/// Person id, the value to write into person-FK columns (HeadTeacherId etc.).
/// </summary>
public class StaffMemberSummaryResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }

    /// <summary>
    /// Employment-derived lifecycle badge (never <c>Archived</c> here — soft-deleted staff are
    /// excluded from the list). Computed in <c>GetStaffMemberSummaries.sql</c> so it sorts/filters.
    /// </summary>
    public StaffStatus Status { get; set; }
}
