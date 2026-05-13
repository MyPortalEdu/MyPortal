using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinAudienceRequest
{
    public BulletinAudienceKind AudienceKind { get; set; }

    // Required iff AudienceKind == StudentGroup.
    public Guid? StudentGroupId { get; set; }
}
