namespace MyPortal.Contracts.Models.People;

public class StaffResponsibilitiesResponse
{
    public List<StaffResponsibilityResponse> Responsibilities { get; set; } = [];

    public List<LookupResponse> ResponsibilityTypes { get; set; } = [];
}
