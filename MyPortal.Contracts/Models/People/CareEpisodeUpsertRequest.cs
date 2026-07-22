namespace MyPortal.Contracts.Models.People;

/// <summary>Write payload for a single care episode. Id is null for a new row; set to reconcile an existing one.</summary>
public class CareEpisodeUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid CaringAuthorityId { get; set; }
    public Guid? LivingArrangementId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
