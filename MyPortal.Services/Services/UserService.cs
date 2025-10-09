using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;

namespace MyPortal.Services.Services;

public class UserService : BaseService, IUserService
{
    private readonly IUserRepository  _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UserService(IAuthorizationService authorizationService, IUserRepository userRepository,
        IPermissionRepository permissionRepository, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager) : base(authorizationService)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.ViewUsers, cancellationToken);
        
        return await _userRepository.GetDetailsByIdAsync(userId, cancellationToken);
    }

    public async Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = _authorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != userId)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.System.ViewUsers, cancellationToken);
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

    public async Task<IdentityResult> SetPasswordAsync(UserSetPasswordDto model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.EditUsers, cancellationToken);
        
        var user = await _userManager.FindByIdAsync(model.UserId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        await _userManager.RemovePasswordAsync(user);
        return await _userManager.AddPasswordAsync(user, model.NewPassword);
    }

    public async Task<IdentityResult> ChangePasswordAsync(UserChangePasswordDto model,
        CancellationToken cancellationToken)
    {
        var currentUserId = _authorizationService.GetCurrentUserId();

        if (currentUserId == null || currentUserId != model.UserId)
        {
            throw new ForbiddenException("You can only change your own password.");
        }

        var user = await _userManager.FindByIdAsync(model.UserId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    }

    public async Task<IdentityResult> CreateUserAsync(CreateUserDto model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.EditUsers, cancellationToken);

        var newUser = new ApplicationUser
        {
            Id = SqlConvention.SequentialGuid(),
            UserName = model.Username,
            Email = model.Email,
            CreatedAt = DateTime.Now,
            IsEnabled = model.IsEnabled,
            UserType = model.UserType,
            PersonId = model.PersonId
        };


        await _userManager.CreateAsync(newUser, model.Password);

        return await UpdateUserRoles(newUser, model.RoleIds);
    }

    public async Task<IdentityResult> UpdateUserAsync(UpdateUserDto model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.EditUsers, cancellationToken);
        
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }
        
        bool userDisabled = user.IsEnabled && !model.IsEnabled;

        user.PersonId = model.PersonId;
        user.IsEnabled = model.IsEnabled;
        user.UserType = model.UserType;

        await UpdateUserRoles(user, model.RoleIds);

        if (userDisabled)
        {
            return await _userManager.UpdateSecurityStampAsync(user);
        }
        
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.EditUsers, cancellationToken);
        
        var user = await _userManager.FindByIdAsync(id.ToString());
        
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }
        
        return await _userManager.DeleteAsync(user);
    }

    private async Task<IdentityResult> UpdateUserRoles(ApplicationUser user, IList<Guid> roleIds)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var newRoles = new List<string>();

        foreach (var roleId in roleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "RoleNotFound",
                    Description = $"Role with ID {roleId} not found."
                });
            }

            if (string.IsNullOrWhiteSpace(role.Name))
            {
                continue;
            }

            await _userManager.AddToRoleAsync(user, role.Name);
            newRoles.Add(role.Name);
        }

        foreach (var userRole in userRoles)
        {
            if (!newRoles.Contains(userRole))
            {
                await _userManager.RemoveFromRoleAsync(user, userRole);
            }
        }

        return IdentityResult.Success;
    }
}