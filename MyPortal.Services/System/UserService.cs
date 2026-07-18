using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.System;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.System;

public class UserService(
    IAuthorizationService authorizationService,
    ILogger<UserService> logger,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IRolePermissionRepository rolePermissionRepository,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IValidationService validationService,
    IUserStatusCache userStatusCache)
    : BaseService(authorizationService, logger), IUserService
{
    public async Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);

        var details = await userRepository.GetDetailsByIdAsync(userId, cancellationToken);

        if (details is null)
        {
            return null;
        }

        details.RoleIds = await userRepository.GetRoleIdsByUserIdAsync(userId, cancellationToken);

        return details;
    }

    public async Task<IList<PermissionResponse>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);

        return await permissionRepository.GetPermissionsByUserIdAsync(userId, cancellationToken);
    }

    public async Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = AuthorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != userId)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);
        }

        var userInfo = await userRepository.GetInfoByIdAsync(userId, cancellationToken);

        if (userInfo == null)
        {
            return null;
        }

        var permissions = await permissionRepository.GetPermissionsByUserIdAsync(userId, cancellationToken);

        userInfo.Permissions = permissions.Select(p => p.Name).ToArray();

        return userInfo;
    }

    public async Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);

        var result = await userRepository.GetUsersAsync(filter, sort, paging, cancellationToken);
        
        return result;
    }

    public async Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.IsSystem)
        {
            throw new SystemEntityException("System users' passwords cannot be reset here.");
        }

        await EnsureActorOutranksTargetAsync(userId, cancellationToken);

        // Reset via a generated token so the new hash replaces the old in a single store
        // operation. The previous remove-then-add could leave a user without a password if
        // the second step failed (e.g. complexity rejection).
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, model.Password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordRequest model,
        CancellationToken cancellationToken)
    {
        var currentUserId = AuthorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != userId)
        {
            throw new ForbiddenException("You can only change your own password.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
    }

    public async Task<(IdentityResult Result, Guid UserId)> CreateAsync(UserUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        await validationService.ValidateAsync(model);

        var newUser = new ApplicationUser
        {
            Id = SqlConvention.SequentialGuid(),
            UserName = model.Username,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow,
            IsEnabled = model.IsEnabled,
            UserType = model.UserType,
            PersonId = model.PersonId,
            CreatedById = AuthorizationService.GetCurrentUserId(),
            CreatedByIpAddress = AuthorizationService.GetCurrentUserIpAddress(),
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedById = AuthorizationService.GetCurrentUserId(),
            LastModifiedByIpAddress = AuthorizationService.GetCurrentUserIpAddress(),
            Version = 1
        };

        using var tx = CreateTransactionScope();

        var result = await userManager.CreateAsync(newUser, model.Password);

        if (!result.Succeeded)
        {
            return (result, Guid.Empty);
        }

        await UpdateUserRoles(newUser, WithDefaultRole(model.UserType, model.RoleIds), cancellationToken);

        tx.Complete();

        return (result, newUser.Id);
    }

    public async Task<IdentityResult> UpdateAsync(Guid userId, UserUpdateRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        await validationService.ValidateAsync(model);

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.IsSystem)
        {
            throw new SystemEntityException("System users cannot be modified.");
        }

        bool userDisabled = user.IsEnabled && !model.IsEnabled;
        bool userTypeChanged = user.UserType != model.UserType;

        // Self-lockout guard: an actor must not be able to lock themselves out in a single edit.
        if (AuthorizationService.GetCurrentUserId() == userId)
        {
            if (userDisabled)
            {
                throw new ForbiddenException("You cannot disable your own account.");
            }

            if (userTypeChanged)
            {
                throw new ForbiddenException("You cannot change your own portal.");
            }
        }

        await EnsureAdminNotLockedOutAsync(user, model, cancellationToken);

        user.PersonId = model.PersonId;
        user.IsEnabled = model.IsEnabled;
        user.UserType = model.UserType;
        user.UserName = model.Username;

        user.LastModifiedAt = DateTime.UtcNow;
        user.LastModifiedById = AuthorizationService.GetCurrentUserId();
        user.LastModifiedByIpAddress = AuthorizationService.GetCurrentUserIpAddress();
        user.Version += 1;

        using var tx = CreateTransactionScope();

        var rolesChanged = await UpdateUserRoles(user, WithDefaultRole(model.UserType, model.RoleIds), cancellationToken);

        IdentityResult result;
        // Rotate the security stamp on any privilege-relevant change. Without this, an active
        // session keeps its claims (UserType, roles) until ValidationInterval expires.
        if (userDisabled || userTypeChanged || rolesChanged)
        {
            result = await userManager.UpdateSecurityStampAsync(user);
        }
        else
        {
            result = await userManager.UpdateAsync(user);
        }

        if (result.Succeeded)
        {
            tx.Complete();
            // Drop the cached IsEnabled so a disable takes effect on the next permission check
            // instead of waiting up to the cache TTL.
            userStatusCache.Invalidate(userId);
        }

        return result;
    }

    public async Task<IdentityResult> DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.IsSystem)
        {
            throw new SystemEntityException("System users cannot be deleted.");
        }

        if (AuthorizationService.GetCurrentUserId() == userId)
        {
            throw new ForbiddenException("You cannot delete your own account.");
        }

        await EnsureNotLastEnabledAdminAsync(userId, cancellationToken);

        var result = await userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            userStatusCache.Invalidate(userId);
        }

        return result;
    }

    // Student and Parent users always carry their portal's default role. Adds it to the requested set
    // if missing; a no-op for staff (and if it's already present).
    private static IList<Guid> WithDefaultRole(UserType userType, IList<Guid> roleIds)
    {
        Guid? defaultRoleId = userType switch
        {
            UserType.Student => SystemRoles.StudentRoleId,
            UserType.Parent => SystemRoles.ParentRoleId,
            _ => null
        };

        if (defaultRoleId is null || roleIds.Contains(defaultRoleId.Value))
        {
            return roleIds;
        }

        return roleIds.Append(defaultRoleId.Value).ToList();
    }

    private async Task<bool> UpdateUserRoles(ApplicationUser user, IList<Guid> roleIds, CancellationToken cancellationToken)
    {
        var changesMade = false;

        var currentRoleNames = await userManager.GetRolesAsync(user);

        var newRoleNames = new List<string>();

        var actorPermissions = await AuthorizationService.GetPermissionsAsync(cancellationToken);
        var permissionsById = (await permissionRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(p => p.Id);

        foreach (var roleId in roleIds)
        {
            var role = await roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
            }

            if (role.UserType != user.UserType)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(UserUpsertRequest.RoleIds),
                        $"Role '{role.Name}' belongs to the {role.UserType} portal and cannot be assigned to a {user.UserType} user.")
                });
            }

            if (string.IsNullOrWhiteSpace(role.Name))
            {
                continue;
            }

            newRoleNames.Add(role.Name);

            if (currentRoleNames.Contains(role.Name))
            {
                continue;
            }

            // Only a newly-added role can escalate: verify the actor holds every permission it grants
            // before assigning it. Removing/keeping an existing role isn't an escalation and is exempt.
            await EnsureActorCanAssignRoleAsync(role, actorPermissions, permissionsById, cancellationToken);

            await userManager.AddToRoleAsync(user, role.Name);
            changesMade = true;
        }

        foreach (var userRole in currentRoleNames)
        {
            if (newRoleNames.Contains(userRole))
            {
                continue;
            }

            await userManager.RemoveFromRoleAsync(user, userRole);
            changesMade = true;
        }

        return changesMade;
    }

    // A staff role may only be assigned by an actor who holds every *administrative* permission it grants
    // — otherwise a user with EditUsers could assign a high-privilege role (e.g. System Administrator) to
    // a staff user or to themselves and escalate. Ordinary functional permissions are not gated, so
    // provisioning works under least privilege: IT support who cannot take a register can still assign
    // the Teacher role. Student/Parent roles are exempt (a user is only assigned roles of their portal).
    private async Task EnsureActorCanAssignRoleAsync(ApplicationRole role, IReadOnlySet<string> actorPermissions,
        IReadOnlyDictionary<Guid, Permission> permissionsById, CancellationToken cancellationToken)
    {
        if (role.UserType != UserType.Staff)
        {
            return;
        }

        var rolePermissions = await rolePermissionRepository.GetByRoleIdAsync(role.Id, cancellationToken);

        var beyondActor = rolePermissions
            .Where(rp => permissionsById.TryGetValue(rp.PermissionId, out var p)
                         && Permissions.IsProtected(p.Name)
                         && !actorPermissions.Contains(p.Name))
            .Select(rp => permissionsById[rp.PermissionId].FriendlyName)
            .ToList();

        if (beyondActor.Count > 0)
        {
            throw new ForbiddenException(
                $"You cannot assign the '{role.Name}' role because it grants administrative permissions you do not hold: {string.Join(", ", beyondActor)}.");
        }
    }

    // Stops an admin with EditUsers from managing (e.g. resetting the password of) a user who holds
    // *administrative* permissions the actor lacks — which would be a takeover of a more-privileged
    // account. Scoped to administrative permissions so routine support (e.g. resetting a teacher's
    // password) still works. Acting on yourself is always allowed (you never outrank yourself).
    private async Task EnsureActorOutranksTargetAsync(Guid targetUserId, CancellationToken cancellationToken)
    {
        if (AuthorizationService.GetCurrentUserId() == targetUserId)
        {
            return;
        }

        var actorPermissions = await AuthorizationService.GetPermissionsAsync(cancellationToken);
        var targetPermissions = await permissionRepository.GetPermissionsByUserIdAsync(targetUserId, cancellationToken);

        if (targetPermissions.Any(p => Permissions.IsProtected(p.Name) && !actorPermissions.Contains(p.Name)))
        {
            throw new ForbiddenException("You cannot manage a user who holds administrative permissions beyond your own.");
        }
    }

    // Blocks any edit that would strip the last enabled System Administrator of that status — disabling
    // them, moving them to another portal, or removing the admin role — which would lock the school out
    // of all system administration irrecoverably.
    private async Task EnsureAdminNotLockedOutAsync(ApplicationUser user, UserUpdateRequest model,
        CancellationToken cancellationToken)
    {
        var adminRole = await roleManager.FindByNameAsync(SystemRoles.SystemAdministratorRoleName);

        if (adminRole is null)
        {
            return;
        }

        var admins = await userManager.GetUsersInRoleAsync(SystemRoles.SystemAdministratorRoleName);

        if (!admins.Any(a => a.Id == user.Id && a.IsEnabled))
        {
            // Target isn't currently an enabled admin, so this edit can't remove the last one.
            return;
        }

        var remainsEnabledAdmin = model.IsEnabled
                                  && model.UserType == UserType.Staff
                                  && model.RoleIds.Contains(adminRole.Id);

        if (remainsEnabledAdmin || admins.Any(a => a.Id != user.Id && a.IsEnabled))
        {
            return;
        }

        throw new ValidationException(new[]
        {
            new ValidationFailure(nameof(UserUpdateRequest.RoleIds),
                "This is the last enabled System Administrator; enable, re-assign, or promote another administrator first.")
        });
    }

    // Delete-time counterpart to EnsureAdminNotLockedOutAsync: a delete always removes the target from
    // the admin pool, so block it when they're the last enabled System Administrator.
    private async Task EnsureNotLastEnabledAdminAsync(Guid targetUserId, CancellationToken cancellationToken)
    {
        var admins = await userManager.GetUsersInRoleAsync(SystemRoles.SystemAdministratorRoleName);

        if (!admins.Any(a => a.Id == targetUserId && a.IsEnabled))
        {
            return;
        }

        if (!admins.Any(a => a.Id != targetUserId && a.IsEnabled))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(UserUpdateRequest.RoleIds),
                    "You cannot delete the last enabled System Administrator.")
            });
        }
    }
}