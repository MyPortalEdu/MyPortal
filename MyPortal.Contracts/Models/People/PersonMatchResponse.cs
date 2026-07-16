namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A candidate existing <c>Person</c> surfaced by the "new staff member" search. Because a
/// Person can hold several subtype roles (staff/student/contact/agent), HR searches existing
/// People first so a joiner who is already on file gets a staff role attached to their existing
/// Person rather than a duplicate Person row.
///
/// <see cref="ExistingStaffMemberId"/> is set when the person already has a (non-deleted) staff
/// record — the UI blocks creating a duplicate staff member and deep-links to the existing
/// profile instead.
/// </summary>
public class PersonMatchResponse
{
    public Guid PersonId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public DateTime? Dob { get; set; }

    /// <summary>The person's existing StaffMember id, or null if they aren't (active) staff.</summary>
    public Guid? ExistingStaffMemberId { get; set; }

    /// <summary>True when the person already has an active staff record. Derived, read-only.</summary>
    public bool IsStaffMember => ExistingStaffMemberId.HasValue;
}
