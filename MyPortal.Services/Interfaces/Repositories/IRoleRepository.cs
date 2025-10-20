using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IRoleRepository : IEntityRepository<Role>
    {
        Task<RoleDetailsDto?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);

        Task<PageResult<RoleSummaryDto>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null,
            PageOptions? paging = null, CancellationToken cancellationToken = default);
    }
}
