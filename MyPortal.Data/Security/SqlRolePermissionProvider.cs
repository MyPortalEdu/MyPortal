using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.Security;

public class SqlRolePermissionProvider(IDbConnectionFactory connectionFactory, IRolePermissionCache cache)
    : IRolePermissionProvider
{
    public async Task<IReadOnlyCollection<string>> GetPermissionsForRolesAsync(IEnumerable<Guid> roleIds,
        CancellationToken ct = default)
    {
        var roles = roleIds.ToArray();
        if (roles.Length == 0) return Array.Empty<string>();

        var all = new List<IReadOnlyCollection<string>>(roles.Length);

        foreach (var role in roles)
        {
            var perms = await cache.GetOrAddAsync(role, FetchAsync, ct);
            all.Add(perms);

            async Task<IReadOnlyCollection<string>> FetchAsync(CancellationToken token)
            {
                const string sql = @"
                    SELECT DISTINCT P.Name
                    FROM Roles R
                    JOIN RolePermissions RP ON RP.RoleId = R.Id
                    JOIN Permissions P ON P.Id = RP.PermissionId
                    WHERE R.Id = @roleId;";

                using var conn = connectionFactory.Create();
                return (await conn.QueryAsync<string>(
                    new CommandDefinition(sql, new { roleId = role }, cancellationToken: token))).ToArray();
            }
        }

        return all.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}