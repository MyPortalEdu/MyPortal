namespace MyPortal.Contracts.Models.People;

/// <summary>Write payload for a single Personal Education Plan. Id is null for a new row; set to reconcile
/// an existing one. ContributorPersonIds is the full desired contributor set (reconciled on save).</summary>
public class PepUpsertRequest
{
    public Guid? Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
    public List<Guid> ContributorPersonIds { get; set; } = [];
}
