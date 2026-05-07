namespace MyPortal.Contracts.Models.Pastoral;

public class RegGroupDetailsResponse
{
    public Guid Id { get; set; }
    public Guid AcademicYearId { get; set; }
    public Guid YearGroupId { get; set; }
    public Guid? RoomId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? Notes { get; set; }
    public IList<StudentGroupSupervisorResponse> Supervisors { get; set; } = new List<StudentGroupSupervisorResponse>();
}