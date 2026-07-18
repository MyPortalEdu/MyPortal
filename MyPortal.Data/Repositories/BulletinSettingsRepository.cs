using System.Data;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class BulletinSettingsRepository(IDbConnectionFactory factory) : IBulletinSettingsRepository
{
    public async Task<IList<BulletinAllowedGroupResponse>> GetAllowedAudienceGroupsAsync(
        CancellationToken cancellationToken)
    {
        using var conn = factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<BulletinAllowedGroupResponse>(
            "[dbo].[usp_bulletin_audience_allowed_group_get_all]", cancellationToken: cancellationToken);
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
        using var conn = factory.Create();
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
        // Single-tenant assumption: the allowlist is school-wide, so an unscoped DELETE
        // is correct. If multi-tenancy is added, this table needs a SchoolId column and
        // the DELETE must be scoped — otherwise saving one school's allowlist will wipe
        // every other school's.
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
