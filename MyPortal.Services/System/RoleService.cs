using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.System;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.System
{
    public class RoleService(
        IAuthorizationService authorizationService,
        ILogger<RoleService> logger,
        IRoleRepository roleRepository,
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository,
        IRolePermissionCache rolePermissionCache,
        RoleManager<ApplicationRole> roleManager,
        IValidationService validationService)
        : BaseService(authorizationService, logger), IRoleService
    {
        public async Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewRoles, cancellationToken);

            var details = await roleRepository.GetDetailsByIdAsync(roleId, cancellationToken);

            if (details is null)
            {
                return null;
            }

            var rolePermissions = await rolePermissionRepository.GetByRoleIdAsync(roleId, cancellationToken);
            details.PermissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

            return details;
        }

        public async Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewRoles, cancellationToken);

            var result = await roleRepository.GetRolesAsync(filter, sort, paging, cancellationToken);

            return result;
        }

        public async Task<(IdentityResult Result, Guid RoleId)> CreateAsync(RoleUpsertRequest model, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditRoles, cancellationToken);

            await validationService.ValidateAsync(model);

            var role = new ApplicationRole
            {
                Id = SqlConvention.SequentialGuid(),
                Name = model.Name,
                Description = model.Description,
                IsSystem = false,
                IsDefault = false,
                UserType = model.UserType
            };

            using var tx = CreateTransactionScope();

            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return (result, Guid.Empty);
            }

            await UpdateRolePermissionsAsync(role, model.PermissionIds, cancellationToken);

            tx.Complete();

            return (result, role.Id);
        }

        public async Task<IdentityResult> UpdateAsync(Guid roleId, RoleUpsertRequest model, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditRoles, cancellationToken);

            await validationService.ValidateAsync(model);

            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.IsSystem)
            {
                throw new SystemEntityException("System roles cannot be modified.");
            }

            // Default roles keep their identity (a school can't rename or delete them) but their
            // permission grants and description stay editable. UserType is immutable by construction
            // (SqlRoleStore never writes it on update).
            if (role.IsDefault && !string.Equals(model.Name, role.Name, StringComparison.Ordinal))
            {
                throw new SystemEntityException("Default roles cannot be renamed.");
            }

            role.Name = model.Name;
            role.Description = model.Description;

            using var tx = CreateTransactionScope();

            var result = await roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return result;
            }

            await UpdateRolePermissionsAsync(role, model.PermissionIds, cancellationToken);

            tx.Complete();

            return result;
        }

        public async Task<IdentityResult> DeleteAsync(Guid roleId, CancellationToken cancellationToken)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditRoles, cancellationToken);

            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.IsSystem)
            {
                throw new SystemEntityException("System roles cannot be deleted.");
            }

            if (role.IsDefault)
            {
                throw new SystemEntityException("Default roles cannot be deleted.");
            }

            using var tx = CreateTransactionScope();

            var rolePermissions = await rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

            foreach (var rolePermission in rolePermissions)
            {
                await rolePermissionRepository.DeleteAsync(rolePermission.Id, cancellationToken);
            }

            var deleteResult = await roleManager.DeleteAsync(role);

            if (deleteResult.Succeeded)
            {
                tx.Complete();
                // Invalidate AFTER successful commit so a failed delete doesn't blow the cache.
                rolePermissionCache.Invalidate(role.Id);
            }

            return deleteResult;
        }

        private async Task<bool> UpdateRolePermissionsAsync(ApplicationRole role, IList<Guid> permissionIds, CancellationToken cancellationToken)
        {
            await EnsurePermissionsMatchRoleAudienceAsync(role, permissionIds, cancellationToken);
            await EnsureActorCanManagePermissionsAsync(role, permissionIds, cancellationToken);

            bool changesMade = false;

            var rolePermissions = await rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

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

                await rolePermissionRepository.InsertAsync(newRolePermission, cancellationToken);
                changesMade = true;
            }

            foreach (var rolePermission in rolePermissions)
            {
                if (permissionIds.Contains(rolePermission.PermissionId))
                {
                    continue;
                }

                await rolePermissionRepository.DeleteAsync(rolePermission.Id, cancellationToken);
                changesMade = true;
            }

            if (changesMade)
            {
                rolePermissionCache.Invalidate(role.Id);
            }

            return changesMade;
        }

        // A role may only hold permissions of its own portal audience: a Student role can't be granted
        // Staff permissions, etc. (SysAdmin's grant-all runs as raw SQL in AuthSeeder and never comes
        // through here, so it stays exempt.)
        private async Task EnsurePermissionsMatchRoleAudienceAsync(ApplicationRole role, IList<Guid> permissionIds,
            CancellationToken cancellationToken)
        {
            if (permissionIds.Count == 0)
            {
                return;
            }

            var permissions = await permissionRepository.GetListAsync(cancellationToken: cancellationToken);
            var byId = permissions.ToDictionary(p => p.Id);

            // Unknown ids used to fall through the audience check and get inserted, surfacing as a raw
            // FK error. Reject them here with a clear message instead.
            if (permissionIds.Any(id => !byId.ContainsKey(id)))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(RoleUpsertRequest.PermissionIds),
                        "One or more permissions do not exist.")
                });
            }

            var mismatch = permissionIds.Any(id => byId.TryGetValue(id, out var p) && p.UserType != role.UserType);

            if (mismatch)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(RoleUpsertRequest.PermissionIds),
                        $"One or more permissions do not belong to the {role.UserType} portal and cannot be assigned to this role.")
                });
            }
        }

        // An actor may only add or remove a Staff role's *administrative* permissions they themselves
        // hold. Adding one above their own would let a user with EditRoles mint access-control power and
        // escalate by assigning it; removing one they don't hold would let them sabotage access control
        // (e.g. strip an admin capability from a role). Only the permissions actually being CHANGED are
        // gated — permissions the role already carried and that stay untouched are left alone, so the
        // actor can still edit the rest of the role without stripping admin perms above their own.
        // Ordinary functional permissions are not gated (provisioning works under least privilege).
        // Student/Parent roles are exempt (cross-portal), and SysAdmin holds every permission.
        private async Task EnsureActorCanManagePermissionsAsync(ApplicationRole role, IList<Guid> permissionIds,
            CancellationToken cancellationToken)
        {
            if (role.UserType != UserType.Staff)
            {
                return;
            }

            var existingIds = (await rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken))
                .Select(rp => rp.PermissionId)
                .ToHashSet();
            var desiredIds = permissionIds.ToHashSet();

            var changed = existingIds.Except(desiredIds)
                .Union(desiredIds.Except(existingIds))
                .ToList();

            if (changed.Count == 0)
            {
                return;
            }

            var actorPermissions = await AuthorizationService.GetPermissionsAsync(cancellationToken);
            var permissions = await permissionRepository.GetListAsync(cancellationToken: cancellationToken);
            var byId = permissions.ToDictionary(p => p.Id);

            var beyondActor = changed
                .Where(id => byId.TryGetValue(id, out var p)
                             && Permissions.IsProtected(p.Name)
                             && !actorPermissions.Contains(p.Name))
                .Select(id => byId[id].FriendlyName)
                .ToList();

            if (beyondActor.Count > 0)
            {
                throw new ForbiddenException(
                    $"You cannot change administrative permissions you do not hold yourself: {string.Join(", ", beyondActor)}.");
            }
        }
    }
}
