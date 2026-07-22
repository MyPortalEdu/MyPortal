namespace MyPortal.Contracts.Models.People;

/// <summary>Write payload for a single SEN need. Id is null for a new row; set to reconcile an existing one.</summary>
public class SenNeedUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid SenTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Rank { get; set; }
}
