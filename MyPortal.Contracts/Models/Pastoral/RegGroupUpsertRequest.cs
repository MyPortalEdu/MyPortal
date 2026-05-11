namespace MyPortal.Contracts.Models.Pastoral;

public class RegGroupUpsertRequest
{
    public RegGroupUpsertRequest()
    {
        Supervisors = new List<StudentGroupSupervisorUpsertRequest>();
    }
    
    public Guid AcademicYearId { get; set; }
    public Guid YearGroupId { get; set; }
    public Guid? RoomId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? Notes { get; set; }
    
    public IList<StudentGroupSupervisorUpsertRequest> Supervisors { get; set; }
}