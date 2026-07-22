namespace MyPortal.Contracts.Models.People;

/// <summary>Write payload for a single SEN provision. Id is null for a new row; set to reconcile an existing one.</summary>
public class SenProvisionUpsertRequest
{
    public Guid? Id { get; set; }
    public Guid SenProvisionTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Frequency { get; set; }
    public decimal? Cost { get; set; }
    public string Note { get; set; } = null!;
}
