namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single employment spell with its contracts. A staff member may have several spells over
/// time (leavers who rejoin keep their prior spells); the most recent is listed first.
/// </summary>
public class StaffEmploymentResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? LeavingReasonId { get; set; }
    public Guid? OriginId { get; set; }
    public Guid? DestinationId { get; set; }
    public string? Notes { get; set; }
    public List<StaffContractResponse> Contracts { get; set; } = [];
}
