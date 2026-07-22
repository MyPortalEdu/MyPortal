namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Responsibilities area: the staff member's designated responsibilities
/// (DSL, First Aider, SENCO, …) plus the type option list. HR-maintained — self / line-manager /
/// HR view, HR edit only.
/// </summary>
public class StaffResponsibilitiesResponse
{
    public List<StaffResponsibilityResponse> Responsibilities { get; set; } = [];

    public List<LookupResponse> ResponsibilityTypes { get; set; } = [];
}
