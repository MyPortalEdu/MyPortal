using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IStaffMemberRepository : IEntityRepository<StaffMember>
{
    /// <summary>
    /// Staff member core + person biographical details for the profile page,
    /// keyed by StaffMember id. Returns null if no staff member matches.
    /// </summary>
    Task<StaffMemberDetailsResponse?> GetDetailsByIdAsync(Guid staffMemberId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Paged staff-only summary for the staff/head-teacher picker. Backed by an
    /// inner join on <c>StaffMembers</c> so people who only exist as
    /// students/contacts are excluded. <see cref="StaffMemberSummaryResponse.Id"/>
    /// is the underlying <c>Person</c> id.
    /// </summary>
    Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
}
