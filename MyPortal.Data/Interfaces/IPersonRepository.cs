using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces
{
    public interface IPersonRepository : IEntityRepository<Person>
    {
        Task<PageResult<PersonSummaryResponse>> GetPeople(FilterOptions? filter = null, SortOptions? sort = null,
            PageOptions? paging = null, bool includeDeleted = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Paged staff-only summary for the staff/head-teacher picker. Backed by
        /// an inner join on <c>StaffMembers</c> so people who only exist as
        /// students/contacts are excluded.
        /// </summary>
        Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
            SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default);
    }
}
