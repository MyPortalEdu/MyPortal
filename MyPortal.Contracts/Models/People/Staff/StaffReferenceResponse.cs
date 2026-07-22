namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A pre-employment reference sought for a staff member.</summary>
public class StaffReferenceResponse
{
    public Guid Id { get; set; }
    public Guid? ReferenceTypeId { get; set; }
    public Guid? ReferenceStatusId { get; set; }
    public string RefereeName { get; set; } = null!;
    public string? RefereeOrganisation { get; set; }
    public string? RefereeEmail { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
}
