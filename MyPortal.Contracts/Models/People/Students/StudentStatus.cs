using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// High-level lifecycle badge for the student header, derived from the student's admission/leaving
/// dates (as of today) rather than stored directly. Precedence runs highest-first: a soft-deleted
/// record is <c>Archived</c>; otherwise a student with no admission date is <c>None</c>, one whose
/// admission is in the future is <c>Future</c>, one who has left (leaving date past) is <c>Leaver</c>,
/// and an admitted, not-yet-left student is <c>Active</c>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StudentStatus
{
    Active,
    Future,
    Leaver,
    None,
    Archived
}
