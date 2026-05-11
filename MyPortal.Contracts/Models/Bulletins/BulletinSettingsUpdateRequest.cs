namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinSettingsUpdateRequest
{
    /// <summary>
    /// The full allowlist of student-group ids that may be selected as a bulletin audience.
    /// Replaces the current set.
    /// </summary>
    public IList<Guid> AllowedAudienceGroupIds { get; set; } = new List<Guid>();
}
