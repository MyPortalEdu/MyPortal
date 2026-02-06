namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinUpsertRequest
{
    public DateTime? ExpiresAt { get; set; }
    
    public required string Title { get; set; }
    
    public required string Detail { get; set; }

    public bool IsPrivate { get; set; }
}