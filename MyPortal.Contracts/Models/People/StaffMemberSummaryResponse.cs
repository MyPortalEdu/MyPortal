namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Lightweight staff row for "pick a staff member" UI (e.g. assigning a head
/// teacher on the school details page). <see cref="Id"/> is the underlying
/// <c>Person</c> id — the same value used in any <c>HeadTeacherId</c> /
/// person-FK column on related tables.
/// </summary>
public class StaffMemberSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
}
