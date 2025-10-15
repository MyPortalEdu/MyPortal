using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Users;

namespace MyPortal.Services.Interfaces.Services;

public interface IUserService
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordDto model,
        CancellationToken cancellationToken);
    
    Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordDto model,  CancellationToken cancellationToken);
    
    Task<IdentityResult> CreateUserAsync(UserUpsertDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> UpdateUserAsync(Guid userId, UserUpsertDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}