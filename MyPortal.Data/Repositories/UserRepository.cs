using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Users;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class UserRepository : EntityRepository<User>, IUserRepository
{
    public UserRepository(IDbConnectionFactory factory) : base(factory)
    {
    }

    public async Task<UserDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        
        var sql = @"[dbo].[sp_user_get_details_by_id]";
        
        var result = await conn.ExecuteStoredProcedureAsync<UserDetailsDto>(sql, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }
}