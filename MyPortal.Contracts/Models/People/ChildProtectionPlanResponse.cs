namespace MyPortal.Contracts.Models.People;

/// <summary>A child protection plan (dated safeguarding record).</summary>
public class ChildProtectionPlanResponse
{
    public Guid Id { get; set; }
    public Guid? LocalAuthorityId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Comment { get; set; }
}
