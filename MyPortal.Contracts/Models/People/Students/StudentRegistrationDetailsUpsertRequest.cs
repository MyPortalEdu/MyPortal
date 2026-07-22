namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// Write payload for the Registration area. Touches only the enrolment / statutory-identifier
/// fields on the student record; admission number is immutable and person bio belongs to the Basic
/// details area. This payload has no way to overwrite them.
/// </summary>
public class StudentRegistrationDetailsUpsertRequest
{
    public Guid? EnrolmentStatusId { get; set; }
    public Guid? BoarderStatusId { get; set; }
    public DateTime? DateStarting { get; set; }

    public string? Upn { get; set; }
    public string? FormerUpn { get; set; }
    public Guid? UpnUnknownReasonId { get; set; }
    public string? Uln { get; set; }
    public string? LaChildId { get; set; }

    public bool IsPartTime { get; set; }
}
