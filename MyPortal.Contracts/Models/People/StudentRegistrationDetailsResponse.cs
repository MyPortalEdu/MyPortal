namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Registration area of the student profile: enrolment/boarder status,
/// admission date, statutory pupil numbers (UPN / former UPN / ULN / LA child id) and part-time
/// flag, plus the active option lists for every picker so the editor is self-contained in one
/// fetch. Pastoral placement (year / reg group / house) is a group-membership concern handled
/// separately. Admission number is read-only (auto-assigned at admission).
/// </summary>
public class StudentRegistrationDetailsResponse
{
    public Guid Id { get; set; }
    public int AdmissionNumber { get; set; }

    public Guid? EnrolmentStatusId { get; set; }
    public Guid? BoarderStatusId { get; set; }
    public DateTime? DateStarting { get; set; }

    public string? Upn { get; set; }
    public string? FormerUpn { get; set; }
    public Guid? UpnUnknownReasonId { get; set; }
    public string? Uln { get; set; }
    public string? LaChildId { get; set; }

    public bool IsPartTime { get; set; }

    // Option lists (active only).
    public List<LookupResponse> EnrolmentStatuses { get; set; } = [];
    public List<LookupResponse> BoarderStatuses { get; set; } = [];
    public List<LookupResponse> UpnUnknownReasons { get; set; } = [];
}
