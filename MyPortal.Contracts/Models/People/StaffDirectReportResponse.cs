namespace MyPortal.Contracts.Models.People;

public class StaffDirectReportResponse
{
    public Guid StaffMemberId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Code { get; set; } = null!;
}
