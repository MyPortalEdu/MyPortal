using Dapper;
using MyPortal.Common.Constants;
using MyPortal.Common.Interfaces;

namespace MyPortal.WebApi;

/// <summary>
/// Fails boot when a row referenced by well-known id (see <see cref="SystemPhotos"/>) is missing.
/// MyPortal.Migrations is a separate executable, so an API pointed at a database that is behind on
/// migrations is an ordinary mistake; without this the first symptom is an FK violation surfacing
/// as a 500 at photo-upload time.
/// </summary>
public static class SystemSeedVerifier
{
    // Table names come from this list, never from callers, so interpolating them is safe.
    private static readonly (string Table, Guid Id, string Description)[] RequiredRows =
    [
        ("Directories", SystemPhotos.DirectoryId,
            "the system Photos directory (seeded by 20260707000000_person_photo_support.sql)"),
        ("DocumentTypes", SystemPhotos.PhotographDocumentTypeId,
            "the \"Photograph\" document type (seeded by 20260707000000_person_photo_support.sql)")
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
