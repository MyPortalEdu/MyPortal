namespace MyPortal.Contracts.Models.Pastoral;

public class YearGroupSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string CurriculumYearGroupName { get; set; } = null!;
    public string? MainSupervisorName { get; set; }
}
