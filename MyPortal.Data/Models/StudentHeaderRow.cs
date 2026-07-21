namespace MyPortal.Data.Models;

/// <summary>
/// Raw row from <c>usp_student_get_header_by_id</c>. The service maps <see cref="IsDeleted"/> plus
/// the admission/leaving dates to <c>StudentStatus</c>, so the enum integer values aren't pinned to
/// a SQL CASE expression.
/// </summary>
public sealed class StudentHeaderRow
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public int AdmissionNumber { get; set; }
    public bool IsDeleted { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PreferredName { get; set; }
    public Guid? PhotoId { get; set; }

    /// <summary>Admission (start) date, or null if not yet admitted.</summary>
    public DateTime? DateStarting { get; set; }

    /// <summary>Leaving date, or null while the student is on roll.</summary>
    public DateTime? DateLeaving { get; set; }
}
