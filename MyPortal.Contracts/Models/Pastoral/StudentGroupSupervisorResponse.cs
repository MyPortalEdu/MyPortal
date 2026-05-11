namespace MyPortal.Contracts.Models.Pastoral;

public class StudentGroupSupervisorResponse
{
    public Guid Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public string StaffMemberName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public bool IsMainSupervisor { get; set; }
}
