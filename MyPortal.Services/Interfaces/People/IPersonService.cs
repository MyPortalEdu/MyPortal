using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People
{
    public interface IPersonService
    {
        /// <summary>
        /// Paged staff-member summary used by the staff/head-teacher picker.
        /// </summary>
        Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default);
    }
}
