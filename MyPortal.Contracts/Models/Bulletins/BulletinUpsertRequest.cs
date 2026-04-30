namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinUpsertRequest
{
    public DateTime? ExpiresAt { get; set; }
    
    public required string Title { get; set; }
    
    public required string Detail { get; set; }

    public bool IsPrivate { get; set; }

    /// <summary>
    /// Optimistic-concurrency token: the version the client believes it is editing.
    /// The server rejects the update if the stored version has moved on.
    /// </summary>
    public long ExpectedVersion { get; set; }
}