using System.Text.Json.Serialization;

namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Relationship of the current viewer to a staff member, surfaced on the profile header so the
/// front end can decide which sections to render alongside the viewer's permission claim — the
/// server still enforces every section endpoint independently. Resolved from the viewer's person
/// identity and the line-management chain. See docs/staff-profile-access.md.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StaffRelationship
{
    /// <summary>No self or line-management link (or the viewer has no person identity).</summary>
    Unrelated,

    /// <summary>The viewer is somewhere above the subject in the line-management chain (transitive).</summary>
    LineManaged,

    /// <summary>The subject is the viewer's own record.</summary>
    Self
}
