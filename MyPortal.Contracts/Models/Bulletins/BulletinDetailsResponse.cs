namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinDetailsResponse
{
    public Guid Id { get; set; }

    public Guid DirectoryId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? PinnedAt { get; set; }

    public string Title { get; set; } = "";

    public string Detail { get; set; } = "";

    public bool RequiresAcknowledgement { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryIcon { get; set; } = "";
    public string CategoryColourCode { get; set; } = "";

    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = "";
    public string CreatedByIpAddress { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public Guid LastModifiedById { get; set; }
    public string LastModifiedByName { get; set; } = "";
    public string LastModifiedByIpAddress { get; set; } = "";
    public DateTime LastModifiedAt { get; set; }

    public long Version { get; set; }

    public IList<BulletinAudienceResponse> Audiences { get; set; } = new List<BulletinAudienceResponse>();

    /// <summary>
    /// Whether the current caller has acknowledged this bulletin. Null when the
    /// bulletin does not require acknowledgement.
    /// </summary>
    public bool? HasAcknowledged { get; set; }

    /// <summary>
    /// Total number of audience members who have acknowledged. Null when the
    /// bulletin does not require acknowledgement.
    /// </summary>
    public int? AcknowledgedCount { get; set; }

    /// <summary>
    /// Number of (non-deleted) documents attached to the bulletin. Counts the
    /// root directory only — bulletins don't expose sub-folders.
    /// </summary>
    public int AttachmentCount { get; set; }
}
