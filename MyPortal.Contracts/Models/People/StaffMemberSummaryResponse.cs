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

    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }

    /// <summary>All name parts concatenated; the target of the Name column filter (not displayed).</summary>
    public string? SearchName { get; set; }

    /// <summary>Job role from the member's primary spell (current if any, else most recent).</summary>
    public string? Role { get; set; }

    /// <summary>Start date of that primary spell.</summary>
    public DateTime? EmploymentStartDate { get; set; }

    /// <summary>Date-only copy of the start date; the target of the Start Date column filter.</summary>
    public DateTime? StartDateOnly { get; set; }

    /// <summary>
    /// Employment-derived lifecycle badge (never <c>Archived</c> here — soft-deleted staff are
    /// excluded from the list). Computed in <c>GetStaffMemberSummaries.sql</c> so it sorts/filters.
    /// </summary>
    public StaffStatus Status { get; set; }
}
