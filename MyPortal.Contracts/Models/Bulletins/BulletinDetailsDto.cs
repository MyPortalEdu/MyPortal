namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinDetailsDto
{
    public Guid Id { get; set; }
    
    public Guid DirectoryId { get; set; }

    public DateTime? ExpiresAt { get; set; }
    
    public string Title { get; set; } = "";
    
    public string Detail { get; set; } = "";

    public bool IsPrivate { get; set; }

    public bool IsApproved { get; set; }
    
    public Guid CreatedById { get; set; }

    public string CreatedByName { get; set; } = "";
    
    public string CreatedByIpAddress { get; set; } = "";
    
    public DateTime CreatedAt { get; set; }
    
    public Guid LastModifiedById { get; set; }

    public string LastModifiedByName { get; set; } = "";
    
    public string LastModifiedByIpAddress { get; set; } = "";
    
    public DateTime LastModifiedAt { get; set; }
}