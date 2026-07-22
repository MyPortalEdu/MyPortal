namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Whole-area replace for the Responsibilities area: the server reconciles every row by id —
/// inserting new assignments, updating matched ids, and soft-deleting anything no longer present.
/// </summary>
public class StaffResponsibilitiesUpsertRequest
{
    public List<StaffResponsibilityUpsertItem> Responsibilities { get; set; } = [];
}
