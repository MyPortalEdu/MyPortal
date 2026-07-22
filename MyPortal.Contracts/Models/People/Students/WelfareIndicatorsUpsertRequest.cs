namespace MyPortal.Contracts.Models.People.Students;

/// <summary>Write payload for the flat welfare indicators held on the Student.</summary>
public class WelfareIndicatorsUpsertRequest
{
    public Guid? PostLookedAfterArrangementId { get; set; }
    public Guid? ServiceChildIndicatorId { get; set; }
    public Guid? YoungCarerIndicatorId { get; set; }
    public Guid? KinshipCareIndicatorId { get; set; }
}
