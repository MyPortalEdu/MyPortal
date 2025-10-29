namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinSummaryDto
{
    public Guid Id { get; set; }

    public DateTime? ExpiresAt { get; set; }
    
    public required string Title { get; set; }
    
    public required string Detail { get; set; }

    public required string CreatedByName { get; set; }
    
    public DateTime? CreatedAt { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsApproved { get; set; }
}