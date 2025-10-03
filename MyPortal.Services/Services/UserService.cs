using Microsoft.AspNetCore.Identity;
using MyPortal.Auth;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Users;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;

namespace MyPortal.Services.Services;

public class UserService : BaseService, IUserService
{
    private readonly IUserRepository  _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IAuthorizationService authorizationService, IUserRepository userRepository,
        UserManager<ApplicationUser> userManager) : base(authorizationService)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.System.ViewUsers, cancellationToken);
        
        return await _userRepository.GetDetailsByIdAsync(id, cancellationToken);
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
        
        await _userManager.CreateAsync(new ApplicationUser
        {
            Id = SqlConvention.SequentialGuid(),
            UserName = model.Username,
            Email = model.Email,
            CreatedAt = DateTime.Now,
            IsEnabled = model.IsEnabled,
            UserType = model.UserType,
            PersonId = model.PersonId
        }, model.Password);
        
        return IdentityResult.Success;
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
}