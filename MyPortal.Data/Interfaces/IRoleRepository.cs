using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces
{
    public interface IRoleRepository : IEntityRepository<Role>
    {
        Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);

        Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null,
            PageOptions? paging = null, CancellationToken cancellationToken = default);
    }
}
