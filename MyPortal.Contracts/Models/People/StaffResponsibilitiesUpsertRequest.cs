namespace MyPortal.Contracts.Models.People;

public class StaffResponsibilitiesUpsertRequest
{
    public List<StaffResponsibilityUpsertItem> Responsibilities { get; set; } = [];
}
