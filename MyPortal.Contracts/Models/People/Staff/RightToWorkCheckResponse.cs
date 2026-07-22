namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A right-to-work check held for a staff member.</summary>
public class RightToWorkCheckResponse
{
    public Guid Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime CheckDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? Notes { get; set; }
}
