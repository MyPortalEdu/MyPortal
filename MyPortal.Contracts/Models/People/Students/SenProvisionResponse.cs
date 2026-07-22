namespace MyPortal.Contracts.Models.People.Students;

/// <summary>A dated SEN provision. EndDate is null for open-ended provision.</summary>
public class SenProvisionResponse
{
    public Guid Id { get; set; }
    public Guid SenProvisionTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Frequency { get; set; }
    public decimal? Cost { get; set; }
    public string Note { get; set; } = null!;
}
