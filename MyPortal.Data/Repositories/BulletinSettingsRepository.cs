using System.Data;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
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
        // usp_bulletin_audience_allowed_group_replace clears the allowlist then re-inserts the
        // supplied ids. The caller's transaction (tx) keeps the DELETE + INSERT atomic. An empty
        // list correctly clears the allowlist (the SP inserts nothing).
        await conn.ExecuteStoredProcedureAsync<int>(
            "[dbo].[usp_bulletin_audience_allowed_group_replace]",
            new { studentGroupIds = studentGroupIds.ToGuidTvp() }, tx,
            cancellationToken: cancellationToken);
    }
}
