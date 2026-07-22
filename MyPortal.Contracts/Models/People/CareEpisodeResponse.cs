namespace MyPortal.Contracts.Models.People;

/// <summary>A looked-after (in-care) episode; the current episode is the open one (EndDate null).</summary>
public class CareEpisodeResponse
{
    public Guid Id { get; set; }
    public Guid CaringAuthorityId { get; set; }
    public Guid? LivingArrangementId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
