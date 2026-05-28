using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffMemberService
{
    /// <summary>
    /// Paged staff-member summary used by the staff/head-teacher picker.
    /// </summary>
    Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Staff member core + person biographical details for the profile page,
    /// keyed by StaffMember id.
    /// </summary>
    Task<StaffMemberDetailsResponse> GetDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates the underlying Person and the StaffMember row in one transaction,
    /// returning the new StaffMember id.
    /// </summary>
    Task<Guid> CreateAsync(StaffMemberUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Updates the StaffMember and its underlying Person biographical fields.</summary>
    Task UpdateAsync(Guid id, StaffMemberUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Soft-deletes the StaffMember row (the underlying Person is left intact).</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
