using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.Security;

public class SqlRoleAccessor(IDbConnectionFactory connectionFactory) : IRoleAccessor
{
    public async Task<IReadOnlyCollection<Guid>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default)
    {
        const string sql = @"SELECT
R.Id
FROM UserRoles UR
JOIN Roles R ON UR.RoleId = R.Id
WHERE UR.UserId = @userId";

        using var conn = connectionFactory.Create();
        var roles = await conn.QueryAsync<Guid>(new CommandDefinition(sql, new { userId }, cancellationToken: ct));
        return roles.Distinct().ToArray();
    }
}