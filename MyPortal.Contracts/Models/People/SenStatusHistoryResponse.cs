namespace MyPortal.Contracts.Models.People;

/// <summary>A dated SEN status row. The open row (null EndDate) is the current status.</summary>
public class SenStatusHistoryResponse
{
    public Guid Id { get; set; }
    public Guid SenStatusId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
