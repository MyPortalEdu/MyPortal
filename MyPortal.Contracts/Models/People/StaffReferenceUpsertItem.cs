namespace MyPortal.Contracts.Models.People;

/// <summary>A reference row in the upsert. Null id is a new row; populated id
/// updates; omitted rows are soft-deleted server-side.</summary>
public class StaffReferenceUpsertItem
{
    public Guid? Id { get; set; }
    public Guid? ReferenceTypeId { get; set; }
    public Guid? ReferenceStatusId { get; set; }
    public string RefereeName { get; set; } = null!;
    public string? RefereeOrganisation { get; set; }
    public string? RefereeEmail { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
}
