namespace MyPortal.Contracts.Models.People;

/// <summary>An overseas conduct check for a staff member who has lived/worked
/// abroad. The country reuses the Nationalities lookup.</summary>
public class StaffOverseasCheckResponse
{
    public Guid Id { get; set; }
    public Guid NationalityId { get; set; }
    public DateTime? CheckedDate { get; set; }
    public bool IsClear { get; set; }
    public string? Notes { get; set; }
}
