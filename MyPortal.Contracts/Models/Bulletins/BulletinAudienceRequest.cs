using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinAudienceRequest
{
    public BulletinAudienceKind AudienceKind { get; set; }
    
    public Guid? StudentGroupId { get; set; }
}
