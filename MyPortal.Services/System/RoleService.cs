using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;

namespace MyPortal.Services.System
{
    public class RoleService : BaseService, IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IRolePermissionCache _rolePermissionCache;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IValidationService _validationService;

        public RoleService(IAuthorizationService authorizationService, ILogger<RoleService> logger, IRoleRepository roleRepository,
            IRolePermissionRepository rolePermissionRepository, IRolePermissionCache rolePermissionCache,
            RoleManager<ApplicationRole> roleManager, IValidationService validationService) : base(authorizationService, logger)
        {
            _roleRepository = roleRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _rolePermissionCache = rolePermissionCache;
            _roleManager = roleManager;
            _validationService = validationService;
        }

        public async Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.System.ViewRoles, cancellationToken);

            return await _roleRepository.GetDetailsByIdAsync(roleId, cancellationToken);
        }

        public async Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.System.ViewRoles, cancellationToken);

            var result = await _roleRepository.GetRolesAsync(filter, sort, paging, cancellationToken);

            return result;
        }

        public async Task<IdentityResult> CreateRoleAsync(RoleUpsertRequest model, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            await _validationService.ValidateAsync(model);

            var role = new ApplicationRole
            {
                Id = SqlConvention.SequentialGuid(),
                Name = model.Name,
                Description = model.Description,
                IsSystem = false
            };

            using var tx = CreateTransactionScope();

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return result;
            }

            await UpdateRolePermissionsAsync(role, model.PermissionIds, cancellationToken);

            tx.Complete();

            return result;
        }

        public async Task<IdentityResult> UpdateRoleAsync(Guid roleId, RoleUpsertRequest model, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            await _validationService.ValidateAsync(model);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

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

            using var tx = CreateTransactionScope();

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return result;
            }

            await UpdateRolePermissionsAsync(role, model.PermissionIds, cancellationToken);

            tx.Complete();

            return result;
        }

        public async Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.System.EditRoles, cancellationToken);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.IsSystem)
            {
                throw new SystemEntityException("System roles cannot be deleted.");
            }

            using var tx = CreateTransactionScope();

            var rolePermissions = await _rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

            foreach (var rolePermission in rolePermissions)
            {
                await _rolePermissionRepository.DeleteAsync(rolePermission.Id, cancellationToken);
            }

            var deleteResult = await _roleManager.DeleteAsync(role);

            if (deleteResult.Succeeded)
            {
                tx.Complete();
                // Invalidate AFTER successful commit so a failed delete doesn't blow the cache.
                _rolePermissionCache.Invalidate(role.Id);
            }

            return deleteResult;
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
