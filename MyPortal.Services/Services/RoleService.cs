using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;

namespace MyPortal.Services.Services
{
    public class RoleService : BaseService, IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IRolePermissionCache _rolePermissionCache;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleService(IAuthorizationService authorizationService, IRoleRepository roleRepository,
            IRolePermissionRepository rolePermissionRepository, IRolePermissionCache rolePermissionCache,
            RoleManager<ApplicationRole> roleManager) : base(authorizationService)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _rolePermissionCache = rolePermissionCache;
            _roleManager = roleManager;
        }

        public async Task<RoleDetailsDto> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.System.ViewRoles, cancellationToken);

            return await _roleRepository.GetDetailsByIdAsync(roleId, cancellationToken);
        }

        public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto model, CancellationToken cancellationToken)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            var role = new ApplicationRole
            {
                Id = SqlConvention.SequentialGuid(),
                Name = model.Name,
                Description = model.Description,
                IsSystem = false
            };

            var result = await _roleManager.CreateAsync(role);

            await UpdateRolePermissionsAsync(role, model.PermissionIds, cancellationToken);

            return result;
        }

        public async Task<IdentityResult> UpdateRoleAsync(UpdateRoleDto model, CancellationToken cancellationToken)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            var role = await _roleManager.FindByIdAsync(model.Id.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.IsSystem)
            {
                throw new SystemEntityException("System roles cannot be modified.");
            }

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await _roleManager.UpdateAsync(role);

            return result;
        }

        public async Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.IsSystem)
            {
                throw new SystemEntityException("System roles cannot be deleted.");
            }

            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

            foreach (var rolePermission in rolePermissions)
            {
                await _rolePermissionRepository.DeleteAsync(rolePermission.Id, cancellationToken);
            }

            _rolePermissionCache.Invalidate(role.Id); var result = await _roleManager.DeleteAsync(role);

            return await _roleManager.DeleteAsync(role);
        }

        private async Task<bool> UpdateRolePermissionsAsync(ApplicationRole role, IList<Guid> permissionIds, CancellationToken cancellationToken)
        {
            bool changesMade = false;

            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

            foreach (var permissionId in permissionIds)
            {
                var existingPermission = rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);

                if (existingPermission != null)
                {
                    continue;
                }

                var newRolePermission = new RolePermission
                {
                    Id = SqlConvention.SequentialGuid(),
                    RoleId = role.Id,
                    PermissionId = permissionId
                };

                await _rolePermissionRepository.InsertAsync(newRolePermission, cancellationToken);
                changesMade = true;
            }

            foreach (var rolePermission in rolePermissions)
            {
                if (permissionIds.Contains(rolePermission.PermissionId))
                {
                    continue;
                }

                await _rolePermissionRepository.DeleteAsync(rolePermission.Id, cancellationToken);
                changesMade = true;
            }

            if (changesMade)
            {
                _rolePermissionCache.Invalidate(role.Id);
            }

            return changesMade;
        }
    }
}
