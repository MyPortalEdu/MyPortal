using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Roles;

namespace MyPortal.Services.Interfaces.Services;

public interface IRoleService
{
    Task<RoleDetailsDto> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task<IdentityResult> CreateRoleAsync(CreateRoleDto model, CancellationToken cancellationToken);
    Task<IdentityResult> UpdateRoleAsync(UpdateRoleDto model, CancellationToken cancellationToken);
    Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken);
}