namespace MyPortal.Contracts.Models.People;

/// <summary>Write payload for a single child protection plan. Id is null for a new row; set to reconcile an existing one.</summary>
public class ChildProtectionPlanUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid? LocalAuthorityId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
