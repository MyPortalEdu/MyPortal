namespace MyPortal.Contracts.Models.People.Students;

public class ChildProtectionPlanResponse
{
    public Guid Id { get; set; }
    public Guid? LocalAuthorityId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
