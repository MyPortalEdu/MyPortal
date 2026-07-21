namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Student profile header. Per-section bodies fetch separately, each gated server-side; the FE
/// composes the sidebar from its own permission claim (student access is flat/role-based, so no
/// viewer relationship is returned — see docs/student-profile-access.md). Year group, reg group,
/// house and enrolment status land with the Registration section slice.
/// </summary>
public class StudentHeaderResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public int AdmissionNumber { get; set; }

    /// <summary>"Title First [Middle] Last", composed from the legal name.</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>Preferred first name (or null).</summary>
    public string? PreferredName { get; set; }

    public Guid? PhotoId { get; set; }
    public StudentStatus Status { get; set; }
}
