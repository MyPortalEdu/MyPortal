using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IStaffMemberRepository : IEntityRepository<StaffMember>
{
    /// <summary>
    /// Header row for the staff profile page — identity, status source, photo, composed
    /// display/preferred names. Returns null if no staff member matches.
    /// </summary>
    Task<StaffMemberHeaderRow?> GetHeaderByIdAsync(Guid staffMemberId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Basic-details area: person bio (no equality fields) + staff code. Null if no match.
    /// </summary>
    Task<StaffBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Paged staff-only summary for the staff/head-teacher picker. Backed by an
    /// inner join on <c>StaffMembers</c> so people who only exist as
    /// students/contacts are excluded.
    /// </summary>
    Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The StaffMember id for a given person, or null if the person isn't (active) staff.
    /// Used to resolve the current user's own staff record for line-management checks.
    /// </summary>
    Task<Guid?> GetStaffMemberIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Search existing People (any subtype) by name for the "new staff member" create flow, so a
    /// joiner who is already on file gets a staff role attached to their existing Person rather
    /// than a duplicate. Each result carries <c>ExistingStaffMemberId</c> when the person is
    /// already (active) staff. <paramref name="like"/> is the caller-built contains pattern.
    /// Capped at 25 rows.
    /// </summary>
    Task<IReadOnlyList<PersonMatchResponse>> SearchPeopleForStaffCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Transitive line-management test: true when <paramref name="managerStaffMemberId"/> appears
    /// anywhere above <paramref name="subjectStaffMemberId"/> in the LineManagerId chain.
    /// Backed by usp_staff_member_is_managed_by.
    /// </summary>
    Task<bool> IsManagedByAsync(Guid subjectStaffMemberId, Guid managerStaffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
