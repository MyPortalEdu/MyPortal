using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The Family area of the student profile — manages the student↔contact relationship links
/// (guardians / carers / emergency contacts) and surfaces derived sibling links. The contact
/// <em>records</em> themselves are managed via the Contacts area; this only owns the join.
/// </summary>
public interface IStudentFamilyService
{
    /// <summary>The student's contacts (priority-ordered) + siblings + relationship-type options.
    /// 404 if the student doesn't exist.</summary>
    Task<StudentFamilyResponse> GetFamilyAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Link a contact to the student. 404 if student/contact missing; 400 if that contact
    /// is already linked to this student. Returns the new relationship id.</summary>
    Task<Guid> AddRelationshipAsync(Guid studentId, StudentContactRelationshipUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>Update a relationship link (type, flags, priority). 404 if the link isn't found on
    /// this student.</summary>
    Task UpdateRelationshipAsync(Guid studentId, Guid relationshipId,
        StudentContactRelationshipUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Unlink a contact from the student. 404 if the link isn't found on this student.</summary>
    Task RemoveRelationshipAsync(Guid studentId, Guid relationshipId, CancellationToken cancellationToken);
}
