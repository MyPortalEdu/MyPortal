namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// Lightweight student row for "pick a student" UI and the student list. <see cref="Id"/> is the
/// Student id (the key for the student profile / CRUD endpoints); <see cref="PersonId"/> is the
/// underlying Person id, the value to write into person-FK columns.
/// </summary>
public class StudentSummaryResponse
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public int AdmissionNumber { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }

    /// <summary>
    /// Admission-derived lifecycle badge (never <c>Archived</c> here — soft-deleted students are
    /// excluded from the list). Computed in <c>GetStudentSummaries.sql</c> so it sorts/filters.
    /// </summary>
    public StudentStatus Status { get; set; }
}
