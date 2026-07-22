namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>An appraisal objective / target held for a staff member.</summary>
public class StaffObjectiveResponse
{
    public Guid Id { get; set; }
    public Guid? ReviewId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? SuccessCriteria { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? StatusId { get; set; }
    public string? ProgressNotes { get; set; }
}
