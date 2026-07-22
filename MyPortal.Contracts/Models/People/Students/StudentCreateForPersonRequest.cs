namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// Write payload for attaching a student role to an existing <c>Person</c>. The person's bio is
/// already on file (created with another subtype role — e.g. a sibling's contact, or a re-admitted
/// former pupil), so no bio fields are supplied here, which would risk silently overwriting the
/// shared Person record. The admission number is auto-assigned. Used by the "new student" flow when
/// the office picks an existing person rather than creating a fresh one.
/// </summary>
public class StudentCreateForPersonRequest
{
    public Guid PersonId { get; set; }
}
