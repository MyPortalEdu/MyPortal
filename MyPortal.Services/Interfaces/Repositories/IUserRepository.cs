using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
}