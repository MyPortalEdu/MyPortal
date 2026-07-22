using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffLineManagerRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffLineManager>(factory, authorizationService), IStaffLineManagerRepository
{
    public async Task<IEnumerable<StaffLineManagerRow>> GetHistoryAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT SLM.[Id], SLM.[LineManagerId], NM.[Name] AS [LineManagerName], MGR.[Code] AS [LineManagerCode], " +
            "SLM.[StartDate], SLM.[EndDate] " +
            "FROM [dbo].[StaffLineManagers] SLM " +
            "LEFT JOIN [dbo].[StaffMembers] MGR ON MGR.[Id] = SLM.[LineManagerId] " +
            "OUTER APPLY [dbo].[fn_person_get_name](MGR.[PersonId], 3, 1, 0) AS NM " +
            "WHERE SLM.[StaffMemberId] = @staffMemberId AND SLM.[IsDeleted] = 0 " +
            "ORDER BY SLM.[StartDate] DESC;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);
            return await conn.QueryAsync<StaffLineManagerRow>(command);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task<StaffLineManager?> GetCurrentAsync(Guid staffMemberId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT TOP 1 [Id], [StaffMemberId], [LineManagerId], [StartDate], [EndDate], [IsDeleted], " +
            "[CreatedById], [CreatedByIpAddress], [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], " +
            "[LastModifiedAt], [Version] FROM [dbo].[StaffLineManagers] " +
            "WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0 " +
            "AND [StartDate] <= @asOf AND ([EndDate] IS NULL OR [EndDate] >= @asOf) " +
            "ORDER BY [StartDate] DESC;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId, asOf }, transaction,
                cancellationToken: cancellationToken);
            return await conn.QueryFirstOrDefaultAsync<StaffLineManager>(command);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
