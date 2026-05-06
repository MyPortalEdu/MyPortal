using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
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

public class UserService : BaseService, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IValidationService _validationService;
    private readonly IUserStatusCache _userStatusCache;

    public UserService(IAuthorizationService authorizationService, ILogger<UserService> logger, IUserRepository userRepository,
        IPermissionRepository permissionRepository, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager, IValidationService validationService,
        IUserStatusCache userStatusCache) : base(authorizationService, logger)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _validationService = validationService;
        _userStatusCache = userStatusCache;
    }

    public async Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);

        return await _userRepository.GetDetailsByIdAsync(userId, cancellationToken);
    }

    public async Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = AuthorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != userId)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);
        }

        var userInfo = await _userRepository.GetInfoByIdAsync(userId, cancellationToken);

        if (userInfo == null)
        {
            return null;
        }

        var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(userId, cancellationToken);

        userInfo.Permissions = permissions.Select(p => p.Name).ToArray();

        return userInfo;
    }

    public async Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, cancellationToken);

        var result = await _userRepository.GetUsersAsync(filter, sort, paging, cancellationToken);
        
        return result;
    }

    public async Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        // Reset via a generated token so the new hash replaces the old in a single store
        // operation. The previous remove-then-add could leave a user without a password if
        // the second step failed (e.g. complexity rejection).
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return await _userManager.ResetPasswordAsync(user, token, model.Password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordRequest model,
        CancellationToken cancellationToken)
    {
        var currentUserId = AuthorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != userId)
        {
            throw new ForbiddenException("You can only change your own password.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
    }

    public async Task<IdentityResult> CreateUserAsync(UserUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        await _validationService.ValidateAsync(model);

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

        var result = await _userManager.CreateAsync(newUser, model.Password);

        if (!result.Succeeded)
        {
            return result;
        }

        await UpdateUserRoles(newUser, model.RoleIds);

        tx.Complete();

        return result;
    }

    public async Task<IdentityResult> UpdateUserAsync(Guid userId, UserUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        await _validationService.ValidateAsync(model);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
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

        var rolesChanged = await UpdateUserRoles(user, model.RoleIds);

        IdentityResult result;
        // Rotate the security stamp on any privilege-relevant change. Without this, an active
        // session keeps its claims (UserType, roles) until ValidationInterval expires.
        if (userDisabled || userTypeChanged || rolesChanged)
        {
            result = await _userManager.UpdateSecurityStampAsync(user);
        }
        else
        {
            result = await _userManager.UpdateAsync(user);
        }

        if (result.Succeeded)
        {
            tx.Complete();
            // Drop the cached IsEnabled so a disable takes effect on the next permission check
            // instead of waiting up to the cache TTL.
            _userStatusCache.Invalidate(userId);
        }

        return result;
    }

    public async Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            _userStatusCache.Invalidate(userId);
        }

        return result;
    }

    private async Task<bool> UpdateUserRoles(ApplicationUser user, IList<Guid> roleIds)
    {
        var changesMade = false;

        var currentRoleNames = await _userManager.GetRolesAsync(user);

        var newRoleNames = new List<string>();

        foreach (var roleId in roleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                throw new NotFoundException("Role not found.");
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

            await _userManager.AddToRoleAsync(user, role.Name);
            changesMade = true;
        }

        foreach (var userRole in currentRoleNames)
        {
            if (newRoleNames.Contains(userRole))
            {
                continue;
            }

            await _userManager.RemoveFromRoleAsync(user, userRole);
            changesMade = true;
        }

        return changesMade;
    }
}