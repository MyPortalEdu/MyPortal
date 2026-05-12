using System.Data;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Data.Interfaces;

namespace MyPortal.Data.Repositories;

public class BulletinSettingsRepository : IBulletinSettingsRepository
{
    private readonly IDbConnectionFactory _factory;

    public BulletinSettingsRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IList<BulletinAllowedGroupResponse>> GetAllowedAudienceGroupsAsync(
        CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    SG.Id          AS StudentGroupId,
    SG.Code        AS Code,
    SG.Description AS Name
FROM dbo.BulletinAudienceAllowedGroups AAG
JOIN dbo.StudentGroups                  SG ON SG.Id = AAG.StudentGroupId
ORDER BY SG.Description;";

        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<BulletinAllowedGroupResponse>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task ReplaceAllowedAudienceGroupsAsync(IList<Guid> studentGroupIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
        {
            await ReplaceCoreAsync(transaction.Connection!, transaction, studentGroupIds, cancellationToken);
            return;
        }

        // No caller-supplied transaction: open a local one so DELETE + INSERT are atomic.
        // Without this, a failure (or cancellation) after the DELETE would leave the
        // allowlist empty.
        using var conn = _factory.Create();
        conn.Open();
        using var localTx = conn.BeginTransaction();
        try
        {
            await ReplaceCoreAsync(conn, localTx, studentGroupIds, cancellationToken);
            localTx.Commit();
        }
        catch
        {
            localTx.Rollback();
            throw;
        }
    }

    private static async Task ReplaceCoreAsync(IDbConnection conn, IDbTransaction tx,
        IList<Guid> studentGroupIds, CancellationToken cancellationToken)
    {
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.BulletinAudienceAllowedGroups;",
            transaction: tx, cancellationToken: cancellationToken));

        if (studentGroupIds.Count == 0)
        {
            return;
        }

        const string insertSql = @"
INSERT INTO dbo.BulletinAudienceAllowedGroups (StudentGroupId)
VALUES (@StudentGroupId);";

        var rows = studentGroupIds.Select(id => new { StudentGroupId = id });

        await conn.ExecuteAsync(new CommandDefinition(insertSql, rows,
            transaction: tx, cancellationToken: cancellationToken));
    }
}
