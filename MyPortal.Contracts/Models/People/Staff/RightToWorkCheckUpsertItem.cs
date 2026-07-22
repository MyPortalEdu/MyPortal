namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A right-to-work row in the upsert. Null id is a new row; populated id
/// updates; omitted rows are soft-deleted server-side. The verifier is recorded
/// server-side as the current user, so it is not part of the payload.</summary>
public class RightToWorkCheckUpsertItem
{
    public Guid? Id { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime CheckDate { get; set; }
    public DateTime? DocumentExpiryDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public string? Notes { get; set; }
}
