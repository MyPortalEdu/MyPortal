using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IStudentRepository : IEntityRepository<Student>
{
    /// <summary>
    /// Header row for the student profile page — identity, admission number, status source (dates),
    /// photo, composed display/preferred names. Returns null if no student matches.
    /// </summary>
    Task<StudentHeaderRow?> GetHeaderByIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Basic-details area: person bio (no cultural/medical fields) + admission number. Null if no match.
    /// </summary>
    Task<StudentBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Paged student summary for the student list / picker. Backed by an inner join on
    /// <c>Students</c> so people who only exist as staff/contacts are excluded.
    /// </summary>
    Task<PageResult<StudentSummaryResponse>> GetStudentsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The Student id for a given person, or null if the person isn't (active) a student. Used to
    /// block a duplicate student role on the same Person.
    /// </summary>
    Task<Guid?> GetStudentIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// The next admission number to assign (current max + 1, starting at 1). Locks the table
    /// (UPDLOCK/HOLDLOCK) so concurrent creates in overlapping transactions don't collide — must be
    /// called inside the create transaction.
    /// </summary>
    Task<int> GetNextAdmissionNumberAsync(CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// The highest UPN serial (final 3 digits) already allocated for the given 9-character prefix
    /// (LA code + establishment number + 2-digit allocation year), or -1 if none. Only permanent
    /// UPNs contribute. Used to suggest the next serial when generating a UPN.
    /// </summary>
    Task<int> GetMaxUpnSerialAsync(string prefix9, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Search existing People (any subtype) by name for the "new student" create flow, so someone
    /// already on file gets a student role attached to their existing Person rather than a duplicate.
    /// Each result carries <c>ExistingStudentId</c> when the person is already (active) a student.
    /// <paramref name="like"/> is the caller-built contains pattern. Capped at 25 rows.
    /// </summary>
    Task<IReadOnlyList<StudentMatchResponse>> SearchPeopleForStudentCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
