using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;

namespace MyPortal.Data.Security;

public class SqlRolePermissionProvider : IRolePermissionProvider
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IRolePermissionCache _cache;

    public SqlRolePermissionProvider(IDbConnectionFactory connectionFactory, IRolePermissionCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsForRolesAsync(IEnumerable<Guid> roleIds,
        CancellationToken ct = default)
    {
        var roles = roleIds.ToArray();
        if (roles.Length == 0) return Array.Empty<string>();

        var all = new List<IReadOnlyCollection<string>>(roles.Length);

        var conn = _connectionFactory.Create();

        foreach (var role in roles)
        {
            var cached = await _cache.GetAsync(role, ct);
            if (cached.Count > 0)
            {
                all.Add(cached);
                continue;
            }

            const string sql = @"
                SELECT DISTINCT P.Name
                FROM Roles R
                JOIN RolePermissions RP ON RP.RoleId = R.Id
                JOIN PermissionIds P ON P.Id = RP.PermissionId
                WHERE R.Id = @roleId;";
            var perms = (await conn.QueryAsync<string>(sql, new { roleId = role })).ToArray();
            _cache.Set(role, perms);
            all.Add(perms);
        }

        return all.SelectMany(x => x).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}