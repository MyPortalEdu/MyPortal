namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinSummaryResponse
{
    public Guid Id { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? PinnedAt { get; set; }

    public required string Title { get; set; }

    public required string Detail { get; set; }

    public required string CreatedByName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public required string CategoryIcon { get; set; }
    public required string CategoryColourCode { get; set; }

    public bool RequiresAcknowledgement { get; set; }

    /// <summary>
    /// Whether the current caller has acknowledged this bulletin. Null when the
    /// bulletin does not require acknowledgement.
    /// </summary>
    public bool? HasAcknowledged { get; set; }
}
