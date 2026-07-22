using System.Data;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IStudentGroupRepository : IEntityRepository<StudentGroup>
{
    Task<IList<StudentGroup>> GetStudentGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Paged summary of student groups in an academic year, with a Kind
    /// discriminator derived from which subtype table claims each base row.
    /// Backs the cross-subtype picker used by bulletins (and any other "pick
    /// a student group" UI).
    /// </summary>
    Task<PageResult<StudentGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    // Wipes the entire pastoral hierarchy for the AY (StudentGroups + Supervisors +
    // YearGroups + RegGroups + Houses) in FK-safe order. Caller must have already
    // verified that no membership/marksheet/etc. data references these groups — this
    // method does not check, it just deletes.
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    // True if any downstream rows (memberships, marksheets, curriculum groups,
    // activities, parent-evening groups, result-set releases) reference this group.
    // StudentGroup is not soft-deletable, so callers should gate hard-delete on this.
    Task<bool> HasDownstreamDataAsync(Guid studentGroupId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    // True if another student group in the same academic year already uses this code.
    // Pass excludeStudentGroupId on update so a group doesn't clash with itself.
    Task<bool> CodeExistsAsync(Guid academicYearId, string code, Guid? excludeStudentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
