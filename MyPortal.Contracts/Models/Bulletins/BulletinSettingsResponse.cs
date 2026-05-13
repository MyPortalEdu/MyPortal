namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinSettingsResponse
{
    public IList<BulletinAllowedGroupResponse> AllowedAudienceGroups { get; set; } =
        new List<BulletinAllowedGroupResponse>();
}

public class BulletinAllowedGroupResponse
{
    public Guid StudentGroupId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
}
