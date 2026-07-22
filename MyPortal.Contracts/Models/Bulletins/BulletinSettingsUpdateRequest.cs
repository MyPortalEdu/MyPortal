namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinSettingsUpdateRequest
{
    public IList<Guid> AllowedAudienceGroupIds { get; set; } = new List<Guid>();
}
