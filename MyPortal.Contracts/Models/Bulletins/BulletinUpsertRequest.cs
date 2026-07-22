namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinUpsertRequest
{
    public DateTime? ExpiresAt { get; set; }

    public Guid CategoryId { get; set; }

    public required string Title { get; set; }

    public required string Detail { get; set; }

    public bool RequiresAcknowledgement { get; set; }
    
    public bool IsPinned { get; set; }
    
    public IList<BulletinAudienceRequest> Audiences { get; set; } = new List<BulletinAudienceRequest>();
    
    public long ExpectedVersion { get; set; }
}
