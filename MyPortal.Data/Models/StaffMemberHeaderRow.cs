namespace MyPortal.Data.Models;

/// <summary>
/// Raw row from <c>usp_staff_member_get_header_by_id</c>. The service maps <see cref="IsDeleted"/>
/// to <c>StaffStatus</c> so the enum integer values aren't pinned to a SQL CASE expression.
/// </summary>
public sealed class StaffMemberHeaderRow
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string Code { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PreferredName { get; set; }
    public Guid? PhotoId { get; set; }
}
