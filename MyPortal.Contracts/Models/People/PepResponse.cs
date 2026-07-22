namespace MyPortal.Contracts.Models.People;

/// <summary>A Personal Education Plan for a looked-after child, with its named contributors.</summary>
public class PepResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }

    public List<PepContributorResponse> Contributors { get; set; } = [];
}
