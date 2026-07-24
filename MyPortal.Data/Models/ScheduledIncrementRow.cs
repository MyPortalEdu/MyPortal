namespace MyPortal.Data.Models;

/// <summary>A scheduled increment with its service term's identity, for lists and the due worklist.</summary>
public class ScheduledIncrementRow
{
    public Guid Id { get; set; }
    public Guid ServiceTermId { get; set; }
    public string ServiceTermCode { get; set; } = null!;
    public string ServiceTermDescription { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? CompletedAt { get; set; }
    public int? AppliedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ScheduledBy { get; set; }
}
