namespace MyPortal.Contracts.Models.Pastoral;

public class HouseUpsertRequest
{
    public HouseUpsertRequest()
    {
        Supervisors = new List<StudentGroupSupervisorUpsertRequest>();
    }
    
    public Guid AcademicYearId { get; set; }
    public string Code { get; set; } = null!;
    public string? ColourCode { get; set; }
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? Notes { get; set; }
    public IList<StudentGroupSupervisorUpsertRequest> Supervisors { get; set; }
}