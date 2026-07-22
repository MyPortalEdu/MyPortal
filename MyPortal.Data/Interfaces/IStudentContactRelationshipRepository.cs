using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Contacts;
using MyPortal.Contracts.Models.People.Students;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentContactRelationshipRepository : IEntityRepository<StudentContactRelationship>
{
    /// <summary>
    /// A student's contact links (priority order, then contact name), projected with the contact's
    /// display name / job title and the relationship-type name.
    /// </summary>
    Task<IReadOnlyList<StudentContactRelationshipResponse>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Derived siblings — other on-roll students who share at least one contact with this student.
    /// </summary>
    Task<IReadOnlyList<SiblingResponse>> GetSiblingsByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// A contact's associated students (the reverse view), projected with the student's display name,
    /// admission number and the relationship-type name.
    /// </summary>
    Task<IReadOnlyList<ContactStudentResponse>> GetByContactIdAsync(Guid contactId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
