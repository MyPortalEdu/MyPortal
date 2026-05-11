namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinUpsertRequest
{
    public DateTime? ExpiresAt { get; set; }

    public Guid CategoryId { get; set; }

    public required string Title { get; set; }

    public required string Detail { get; set; }

    public bool RequiresAcknowledgement { get; set; }

    /// <summary>
    /// Pinned bulletins sort to the top of the feed. Distinct from publish: pinning
    /// is an additional gesture that surfaces an already-published bulletin.
    /// </summary>
    public bool IsPinned { get; set; }

    /// <summary>
    /// The audiences this bulletin targets. At least one entry is required; the
    /// validator enforces this and rejects malformed entries (e.g. StudentGroup
    /// kind with no StudentGroupId).
    /// </summary>
    public IList<BulletinAudienceRequest> Audiences { get; set; } = new List<BulletinAudienceRequest>();

    /// <summary>
    /// Optimistic-concurrency token: the version the client believes it is editing.
    /// The server rejects the update if the stored version has moved on.
    /// </summary>
    public long ExpectedVersion { get; set; }
}
