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

namespace MyPortal.Services.System;

public class UserService(
    IAuthorizationService authorizationService,
    ILogger<UserService> logger,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
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

        await UpdateUserRoles(newUser, WithDefaultRole(model.UserType, model.RoleIds));

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

        user.PersonId = model.PersonId;
        user.IsEnabled = model.IsEnabled;
        user.UserType = model.UserType;
        user.UserName = model.Username;

        user.LastModifiedAt = DateTime.UtcNow;
        user.LastModifiedById = AuthorizationService.GetCurrentUserId();
        user.LastModifiedByIpAddress = AuthorizationService.GetCurrentUserIpAddress();
        user.Version += 1;

        using var tx = CreateTransactionScope();

        var rolesChanged = await UpdateUserRoles(user, WithDefaultRole(model.UserType, model.RoleIds));

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

    private async Task<bool> UpdateUserRoles(ApplicationUser user, IList<Guid> roleIds)
    {
        var changesMade = false;

        var currentRoleNames = await userManager.GetRolesAsync(user);

        var newRoleNames = new List<string>();

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
}