using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// High-level lifecycle badge for the staff header, derived from the member's employment spells
/// (as of today) rather than stored directly. Precedence runs highest-first: a soft-deleted record
/// is <c>Archived</c>; otherwise a member with a current spell is <c>Active</c>, one whose only
/// spell(s) start in the future is <c>Future</c>, one whose every spell has ended is <c>Leaver</c>,
/// and one with no employment on record is <c>None</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StaffStatus
{
    Active,
    Future,
    Leaver,
    None,
    Archived
}
