namespace MyPortal.Data.Models;

/// <summary>
/// Raw row from <c>usp_staff_member_get_header_by_id</c>. The service maps <see cref="IsDeleted"/>
/// plus the three employment flags to <c>StaffStatus</c>, so the enum integer values aren't pinned
/// to a SQL CASE expression. The flags are date aggregates over the member's non-deleted spells.
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

    /// <summary>A spell has started and not yet ended (open-ended counts) as of today.</summary>
    public bool HasCurrentEmployment { get; set; }

    /// <summary>A spell starts in the future.</summary>
    public bool HasFutureEmployment { get; set; }

    /// <summary>Any non-deleted spell exists at all.</summary>
    public bool HasAnyEmployment { get; set; }
}
