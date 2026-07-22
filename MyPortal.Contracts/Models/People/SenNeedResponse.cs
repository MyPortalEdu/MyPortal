namespace MyPortal.Contracts.Models.People;

/// <summary>A dated, ranked SEN need (Rank 1 = primary).</summary>
public class SenNeedResponse
{
    public Guid Id { get; set; }
    public Guid SenTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Rank { get; set; }
}
