namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// A candidate existing <c>Person</c> surfaced by the "new student" search. Because a Person can
/// hold several subtype roles (staff/student/contact/agent), the office searches existing People
/// first so someone already on file (a sibling's contact, a re-admitted former pupil, ...) gets a
/// student role attached to their existing Person rather than a duplicate Person row.
///
/// <see cref="ExistingStudentId"/> is set when the person already has a (non-deleted) student
/// record — the UI blocks creating a duplicate student and deep-links to the existing profile.
/// </summary>
public class StudentMatchResponse
{
    public Guid PersonId { get; set; }
    public string? Title { get; set; }
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = null!;
    public string? PreferredFirstName { get; set; }
    public string? PreferredLastName { get; set; }
    public DateTime? Dob { get; set; }

    /// <summary>The person's existing Student id, or null if they aren't (active) a student.</summary>
    public Guid? ExistingStudentId { get; set; }

    /// <summary>True when the person already has an active student record. Derived, read-only.</summary>
    public bool IsStudent => ExistingStudentId.HasValue;
}
