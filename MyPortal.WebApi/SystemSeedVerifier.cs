using Dapper;
using MyPortal.Common.Constants;
using MyPortal.Common.Interfaces;

namespace MyPortal.WebApi;

/// <summary>
/// Fails boot when a seeded row the code depends on is missing. Migrations run as a separate
/// executable, so an API pointed at a database that is behind on migrations is an ordinary mistake;
/// without this the first symptom is a cryptic failure (AuthSeeder's single-row role lookup, or an FK
/// violation at first use). Runs BEFORE AuthSeeder, which depends on the System Administrator role.
/// </summary>
public static class SystemSeedVerifier
{
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var connFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        using var conn = connFactory.Create();

        var missing = new List<string>();

        // Portal roles auto-assigned by fixed id in UserService.
        foreach (var (id, name) in new[]
                 {
                     (SystemRoles.StudentRoleId, "Student"),
                     (SystemRoles.ParentRoleId, "Parent")
                 })
        {
            var found = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.Roles WHERE Id = @id", new { id });
            if (found == 0)
            {
                missing.Add($"  - the {name} role (expected Roles.Id = {id})");
            }
        }

        // System Administrator is resolved by name (AuthSeeder); its id varies across databases.
        var normalized = SystemRoles.SystemAdministratorRoleName.ToUpperInvariant();
        var sysAdmin = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM dbo.Roles WHERE NormalizedName = @n", new { n = normalized });
        if (sysAdmin == 0)
        {
            missing.Add($"  - the {SystemRoles.SystemAdministratorRoleName} role (expected by NormalizedName)");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "The database is missing seeded rows this build requires. It is most likely behind on " +
                "migrations — run the MyPortal.Migrations project against it." +
                Environment.NewLine + string.Join(Environment.NewLine, missing));
        }
    }
}
