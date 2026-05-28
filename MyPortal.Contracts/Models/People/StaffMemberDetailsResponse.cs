namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Core staff-member record plus the underlying person's biographical details,
/// backing the staff profile page. Employment, contract, DBS/right-to-work and
/// qualification records hang off the staff member but are surfaced through
/// their own endpoints rather than inlined here.
/// </summary>
public class StaffMemberDetailsResponse
{
    /// <summary>The StaffMember id (the key for staff CRUD endpoints).</summary>
    public Guid Id { get; set; }

    /// <summary>The underlying Person id.</summary>
    public Guid PersonId { get; set; }

    public PersonDetailsResponse Person { get; set; } = null!;

    public Guid? LineManagerId { get; set; }

    public Guid? InductionStatusId { get; set; }

    public string Code { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankAccount { get; set; }

    public string? BankSortCode { get; set; }

    public string? NiNumber { get; set; }

    public string? TeacherReferenceNumber { get; set; }

    public string? Qualifications { get; set; }

    public bool IsTeachingStaff { get; set; }

    public bool HasQts { get; set; }

    public DateTime? QtsAwardedDate { get; set; }

    public DateTime? InductionStartDate { get; set; }

    public DateTime? InductionCompletedDate { get; set; }

    public bool HasDisability { get; set; }

    public string? DisabilityDetails { get; set; }

    public int PpaPeriodsPerWeek { get; set; }

    public bool IsDeleted { get; set; }
}
