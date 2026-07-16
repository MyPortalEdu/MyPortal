namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Absences &amp; Leave area: the absence/leave records the viewer is
/// permitted to see (confidential rows are filtered out for line-manager viewers — only HR and
/// the staff member themselves see them) plus the type option lists. Health data — self / line
/// manager / HR view, line-manager / HR edit (no self-edit).
/// </summary>
public class StaffAbsencesResponse
{
    public List<StaffAbsenceResponse> Absences { get; set; } = [];

    public List<LookupResponse> AbsenceTypes { get; set; } = [];
    public List<LookupResponse> IllnessTypes { get; set; } = [];
}
