namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Management section of the Basic Details area: the staff member's line
/// manager plus their direct reports. <see cref="ManagerOptions"/> (the active-staff picker list)
/// is populated only when the caller may edit — a view-only viewer doesn't get the full roster.
/// </summary>
public class StaffManagementResponse
{
    public Guid? LineManagerId { get; set; }
    public string? LineManagerName { get; set; }
    public string? LineManagerCode { get; set; }

    public List<StaffDirectReportResponse> DirectReports { get; set; } = [];

    /// <summary>The full reporting-line history, newest first.</summary>
    public List<StaffLineManagerHistoryResponse> History { get; set; } = [];

    public bool CanEdit { get; set; }
    public List<LookupResponse> ManagerOptions { get; set; } = [];
}
