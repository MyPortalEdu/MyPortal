using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Roles;

namespace MyPortal.Services.Interfaces.Services;

public interface IRoleService
{
    Task<RoleDetailsDto?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task<IdentityResult> CreateRoleAsync(RoleUpsertDto model, CancellationToken cancellationToken);
    Task<IdentityResult> UpdateRoleAsync(Guid roleId, RoleUpsertDto model, CancellationToken cancellationToken);
    Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken);
}