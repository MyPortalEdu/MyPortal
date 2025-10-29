using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class UserRepository : EntityRepository<User>, IUserRepository
{
    protected UserRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(factory,
        authorizationService)
    {
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        
        var sql = @"[dbo].[sp_user_get_details_by_id]";
        
        var result = await conn.ExecuteStoredProcedureAsync<UserDetailsDto>(sql, new { userId }, 
            cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<UserInfoDto?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_user_get_info_by_id]";

        var result = await conn.ExecuteStoredProcedureAsync<UserInfoDto>(sql, new { userId }, 
            cancellationToken: cancellationToken);
        return result.FirstOrDefault();
    }

    public async Task<PageResult<UserSummaryDto>> GetUsersAsync(FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("System.Users.GetUsers.sql");

        var result = await GetListPagedAsync<UserSummaryDto>(sql, null, filter, sort, paging, false, cancellationToken);
        
        return result;
    }
}