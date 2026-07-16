namespace MyPortal.Contracts.Models.People;

/// <summary>An appraisal review cycle held for a staff member.</summary>
public class PerformanceReviewResponse
{
    public Guid Id { get; set; }
    public string? CycleName { get; set; }
    public Guid? ReviewerId { get; set; }
    public Guid? StatusId { get; set; }
    public DateTime? ReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public Guid? OverallRatingId { get; set; }
    public string? Summary { get; set; }
}
