namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Create/update payload. <see cref="Person"/> populates the underlying Person row; the rest is
/// staff-specific. Create writes both rows in one transaction.
/// </summary>
public class StaffMemberUpsertRequest
{
    public PersonUpsertRequest Person { get; set; } = new();

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
}
