namespace MyPortal.Contracts.Models.Pastoral;

// The fields every IStudentGroupEntity (House, YearGroup, RegGroup, Activity)
// shares via its backing StudentGroup row. Pastoral upsert request DTOs project
// down to this so StudentGroupService doesn't have to know about the host entity.
public class StudentGroupUpsertCore
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? Notes { get; set; }
}
