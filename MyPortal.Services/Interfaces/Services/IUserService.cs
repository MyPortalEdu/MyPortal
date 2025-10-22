using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Users;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

public interface IUserService
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<PageResult<UserSummaryDto>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);

    Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordDto model,
        CancellationToken cancellationToken);
    
    Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordDto model,  CancellationToken cancellationToken);
    
    Task<IdentityResult> CreateUserAsync(UserUpsertDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> UpdateUserAsync(Guid userId, UserUpsertDto model, CancellationToken cancellationToken);
    
    Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}