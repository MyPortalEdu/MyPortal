namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Professional Details area. The qualifications list is a
/// whole-collection replace: the server diffs against what's stored — inserting new rows,
/// updating matched ids, and soft-deleting any row no longer present.
/// </summary>
public class StaffProfessionalDetailsUpsertRequest
{
    public bool IsTeachingStaff { get; set; }
    public Guid? TeacherCategoryId { get; set; }
    public Guid? TeacherStatusId { get; set; }
    public bool EligibleForSwr { get; set; }
    public bool HasQts { get; set; }
    public bool HasHlta { get; set; }
    public bool HasQtls { get; set; }
    public bool HasEyts { get; set; }
    public bool IsSeniorLeadership { get; set; }

    public string? TeacherReferenceNumber { get; set; }
    public Guid? QtsRouteId { get; set; }
    public DateTime? QtsAwardedDate { get; set; }

    public Guid? InductionStatusId { get; set; }
    public DateTime? InductionStartDate { get; set; }
    public DateTime? InductionCompletedDate { get; set; }

    public string? QualificationsSummary { get; set; }

    public List<StaffQualificationUpsertItem> Qualifications { get; set; } = [];
}
