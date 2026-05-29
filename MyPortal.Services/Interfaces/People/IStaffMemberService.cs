using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffMemberService
{
    /// <summary>Paged staff summary for the staff/head-teacher picker.</summary>
    Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>Staff profile header — identity + status + viewer relationship.
    /// 403 if no view-basic-details scope covers the subject.</summary>
    Task<StaffMemberHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates Person + StaffMember in one transaction; returns the StaffMember id.</summary>
    Task<Guid> CreateAsync(StaffMemberUpsertRequest model, CancellationToken cancellationToken);

    Task UpdateAsync(Guid id, StaffMemberUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Soft-deletes the StaffMember row; the underlying Person is left intact.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
