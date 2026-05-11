namespace MyPortal.Contracts.Models.Pastoral;

public class StudentGroupSupervisorUpsertRequest
{
    public Guid StaffMemberId { get; set; }
    public string Title { get; set; } = null!;
    public bool MainSupervisor { get; set; }
}