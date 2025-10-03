using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.Users;

namespace MyPortal.Services.Interfaces.Services;

public interface IUserService
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IdentityResult> ChangePasswordAsync(UserChangePasswordDto model,
        CancellationToken cancellationToken);
    
    Task<IdentityResult> SetPasswordAsync(UserSetPasswordDto model,  CancellationToken cancellationToken);
    
    Task<IdentityResult> CreateUserAsync(CreateUserDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> UpdateUserAsync(UpdateUserDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> DeleteUserAsync(Guid id, CancellationToken cancellationToken);
}