namespace MyPortal.Common.Enums;

/// <summary>
/// Discriminator for the unified student-groups listing. Each value maps to a
/// subtype table that holds a 1:1 FK back to <c>dbo.StudentGroups</c>. The
/// column is materialised in SQL with a CASE expression rather than stored on
/// the base table — adding a new subtype means adding a new branch there.
/// </summary>
public enum StudentGroupKind : byte
{
    /// <summary>Fallback: row exists in StudentGroups but no known subtype claims it.</summary>
    Other = 0,
    House = 1,
    YearGroup = 2,
    RegGroup = 3,
    CurriculumGroup = 4
}
