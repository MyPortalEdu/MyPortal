namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A staff member who reports directly to the subject (their <c>LineManagerId</c> is the subject).
/// Lightweight — just enough to render a linked list on the Management section.
/// </summary>
public class StaffDirectReportResponse
{
    public Guid StaffMemberId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Code { get; set; } = null!;
}
