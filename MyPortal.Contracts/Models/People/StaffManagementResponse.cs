namespace MyPortal.Contracts.Models.People;

public class StaffManagementResponse
{
    public Guid? LineManagerId { get; set; }
    public string? LineManagerName { get; set; }
    public string? LineManagerCode { get; set; }

    public List<StaffDirectReportResponse> DirectReports { get; set; } = [];

    public List<StaffLineManagerHistoryResponse> History { get; set; } = [];

    public bool CanEdit { get; set; }
    public List<LookupResponse> ManagerOptions { get; set; } = [];
}
