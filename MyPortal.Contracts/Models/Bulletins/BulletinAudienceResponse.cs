using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinAudienceResponse
{
    public Guid Id { get; set; }
    public BulletinAudienceKind AudienceKind { get; set; }
    public Guid? StudentGroupId { get; set; }
    public string? StudentGroupName { get; set; }
}
