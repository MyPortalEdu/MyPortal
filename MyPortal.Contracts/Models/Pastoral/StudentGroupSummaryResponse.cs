using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Pastoral;

/// <summary>
/// Unified row for the cross-subtype student-groups list (used by the bulletin
/// audience picker and any other "pick a group" UI). The <see cref="Kind"/>
/// discriminator distinguishes Houses / YearGroups / RegGroups / etc. so the
/// picker can show a Type column and filter on it.
/// </summary>
public class StudentGroupSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public StudentGroupKind Kind { get; set; }
}
