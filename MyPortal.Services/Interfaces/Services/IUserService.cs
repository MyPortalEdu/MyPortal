using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Users;

namespace MyPortal.Services.Interfaces.Services;

public interface IUserService
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IdentityResult> ChangePasswordAsync(UserChangePasswordDto model,
        CancellationToken cancellationToken);
    
    Task<IdentityResult> SetPasswordAsync(UserSetPasswordDto model,  CancellationToken cancellationToken);
    
    Task<IdentityResult> CreateUserAsync(CreateUpsertUserDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> UpdateUserAsync(UpdateUpsertUserDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}