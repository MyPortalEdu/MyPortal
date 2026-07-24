using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class PostRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Post>(factory, authorizationService), IPostRepository
{
    public async Task<IEnumerable<PostRow>> GetAllWithUsageAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT P.[Id], P.[Reference], P.[Description], P.[PostCategoryId], P.[ServiceTermId], " +
            "P.[SwrPostCode], P.[EstablishedFte], " +
            "(SELECT COUNT(*) FROM [dbo].[StaffContracts] SC WHERE SC.[PostId] = P.[Id] AND SC.[IsDeleted] = 0) " +
            "AS [ContractCount] " +
            "FROM [dbo].[Posts] P WHERE P.[IsDeleted] = 0 ORDER BY P.[Reference];";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, null, transaction, cancellationToken: cancellationToken);
            return await conn.QueryAsync<PostRow>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<bool> ReferenceExistsAsync(string reference, Guid? excludePostId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM [dbo].[Posts] " +
            "WHERE [Reference] = @reference AND [IsDeleted] = 0 " +
            "AND (@excludePostId IS NULL OR [Id] <> @excludePostId)) THEN 1 ELSE 0 END AS bit);";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { reference, excludePostId }, transaction,
                cancellationToken: cancellationToken);
            return await conn.ExecuteScalarAsync<bool>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }
}
