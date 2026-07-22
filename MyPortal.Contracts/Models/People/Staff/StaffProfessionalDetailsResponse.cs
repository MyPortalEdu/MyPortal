namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Read payload for the Professional Details area: the staff member's teaching-status and
/// QTS / induction fields, their structured qualifications, and the active option lists for
/// every picker so the editor is self-contained in one fetch. Relationship-scoped —
/// Own / Managed / All view; HR or line-manager edit (no self-edit, HR-verified).
/// </summary>
public class StaffProfessionalDetailsResponse
{
    // Teaching status / workforce-census flags.
    public bool IsTeachingStaff { get; set; }
    public bool HasQts { get; set; }
    public bool HasHlta { get; set; }
    public bool HasQtls { get; set; }
    public bool HasEyts { get; set; }
    public bool IsSeniorLeadership { get; set; }

    // Qualified Teacher Status.
    public string? TeacherReferenceNumber { get; set; }
    public Guid? QtsRouteId { get; set; }
    public DateTime? QtsAwardedDate { get; set; }

    // Statutory induction (ECT).
    public Guid? InductionStatusId { get; set; }
    public DateTime? InductionStartDate { get; set; }
    public DateTime? InductionCompletedDate { get; set; }

    // Free-text summary kept alongside the structured qualifications list.
    public string? QualificationsSummary { get; set; }

    public List<StaffQualificationResponse> Qualifications { get; set; } = [];

    // Option lists (active only).
    public List<LookupResponse> QtsRoutes { get; set; } = [];
    public List<LookupResponse> InductionStatuses { get; set; } = [];
    public List<LookupResponse> QualificationLevels { get; set; } = [];
    public List<LookupResponse> ClassesOfDegree { get; set; } = [];
}
