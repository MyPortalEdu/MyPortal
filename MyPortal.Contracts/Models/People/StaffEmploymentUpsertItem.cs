namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One employment spell in the employment-details replace payload. A null <see cref="Id"/> is a
/// new spell; a populated <see cref="Id"/> updates the existing one. Spells present in the
/// database but absent from the payload are soft-deleted, along with their contracts.
/// </summary>
public class StaffEmploymentUpsertItem
{
    public Guid? Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ContinuousServiceStartDate { get; set; }
    public DateTime? LocalAuthorityStartDate { get; set; }
    public Guid? LeavingReasonId { get; set; }
    public Guid? OriginId { get; set; }
    public Guid? DestinationId { get; set; }
    public string? PreviousEmployer { get; set; }
    public string? NextEmployer { get; set; }
    public string? Notes { get; set; }
    public List<StaffContractUpsertItem> Contracts { get; set; } = [];
}
