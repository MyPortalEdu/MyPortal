using Dapper;
using MyPortal.Common.Constants;
using MyPortal.Common.Interfaces;

namespace MyPortal.WebApi;

/// <summary>
/// Fails boot when a row the code references by well-known id is missing. MyPortal.Migrations is a
/// separate executable, so an API pointed at a database that is behind on migrations is an ordinary
/// mistake; without this the first symptom is an FK violation surfacing as a 500 at the point of use
/// (e.g. photo upload, or school creation).
/// </summary>
public static class SystemSeedVerifier
{
    // Table names come from this list, never from callers, so interpolating them is safe.
    private static readonly (string Table, Guid Id, string Description)[] RequiredRows =
    [
        ("Directories", SystemPhotos.DirectoryId,
            "the system Photos directory (seeded by 20260707000000_person_photo_support.sql)"),
        ("DocumentTypes", SystemPhotos.PhotographDocumentTypeId,
            "the \"Photograph\" document type (seeded by 20260707000000_person_photo_support.sql)"),
        ("AgencyTypes", AgencyTypes.EducationalProvider,
            "the Educational Provider agency type (seeded by 20251101000300_seed_uk_data.sql) — every school create references it"),
        ("Users", SystemUsers.SentinelUserId,
            "the system sentinel user (seeded by 20251101000050_seed_system_user.sql) — seed migrations reference it for row ownership")
    ];

    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var connFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using var conn = connFactory.Create();

        var missing = new List<string>();

        foreach (var (table, id, description) in RequiredRows)
        {
            var found = await conn.ExecuteScalarAsync<int>(
                $"SELECT COUNT(1) FROM dbo.[{table}] WHERE Id = @id", new { id });

            if (found == 0)
            {
                missing.Add($"  - {description}: expected {table}.Id = {id}");
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
                "The database is missing rows that this build references by well-known id. It is " +
                "most likely behind on migrations — run the MyPortal.Migrations project against it." +
                Environment.NewLine + string.Join(Environment.NewLine, missing));
        }
    }
}
