namespace MyPortal.Contracts.Models.People.Students;

public class CareEpisodeUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid CaringAuthorityId { get; set; }
    public Guid? LivingArrangementId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
