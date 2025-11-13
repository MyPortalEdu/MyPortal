using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Roles;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

public interface IRoleService
{
    Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);

    Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null, CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateRoleAsync(RoleUpsertRequest model, CancellationToken cancellationToken);
    Task<IdentityResult> UpdateRoleAsync(Guid roleId, RoleUpsertRequest model, CancellationToken cancellationToken);
    Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken);
}