using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IUserRepository : IEntityRepository<User>
{
    Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<PageResult<UserSummaryDto>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);
}