using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class UserRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<User>(factory,
        authorizationService), IUserRepository
{
    public async Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        
        var sql = @"[dbo].[usp_user_get_details_by_id]";
        
        var result = await conn.ExecuteStoredProcedureAsync<UserDetailsResponse>(sql, new { userId },
            cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<IList<Guid>> GetRoleIdsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var result = await conn.ExecuteStoredProcedureAsync<Guid>("[dbo].[usp_user_role_get_by_user_id]",
            new { userId }, cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_user_get_info_by_id]";

        var result = await conn.ExecuteStoredProcedureAsync<UserInfoResponse>(sql, new { userId }, 
            cancellationToken: cancellationToken);
        return result.FirstOrDefault();
    }

    public async Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("System.Users.GetUserSummaries.sql");

        var result = await GetListPagedAsync<UserSummaryResponse>(sql, null, filter, sort, paging, false, cancellationToken);
        
        return result;
    }
}