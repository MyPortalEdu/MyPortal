namespace MyPortal.Contracts.Models.People;

/// <summary>A review-cycle row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are soft-deleted server-side.</summary>
public class PerformanceReviewUpsertItem
{
    public Guid? Id { get; set; }
    public string? CycleName { get; set; }
    public Guid? ReviewerId { get; set; }
    public Guid? StatusId { get; set; }
    public DateTime? ReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public Guid? OverallRatingId { get; set; }
    public string? Summary { get; set; }
}
