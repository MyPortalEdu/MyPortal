using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Users;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

public interface IUserService
{
    Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);

    Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordRequest model,
        CancellationToken cancellationToken);
    
    Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordRequest model,  CancellationToken cancellationToken);
    
    Task<IdentityResult> CreateUserAsync(UserUpsertRequest model, CancellationToken cancellationToken);
    
    Task<IdentityResult> UpdateUserAsync(Guid userId, UserUpsertRequest model, CancellationToken cancellationToken);
    
    Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}