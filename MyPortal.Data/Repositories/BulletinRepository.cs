using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Data.VisibilityScopes;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class BulletinRepository : EntityRepository<Bulletin>, IBulletinRepository
{
    public BulletinRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }

    public async Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var p = new
        {
            bulletinId,
            currentUserId = scope.CurrentUserId,
            isStaff = scope.IsStaff,
            isPupil = scope.IsPupil,
            isParent = scope.IsParent,
            canView = scope.CanView,
            canEdit = scope.CanEdit,
            canPin = scope.CanPin
        };

        var command = new CommandDefinition("[dbo].[usp_bulletin_get_details_by_id]", p,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<BulletinDetailsResponse>();
        if (header is null)
        {
            return null;
        }

        header.Audiences = (await reader.ReadAsync<BulletinAudienceResponse>()).ToList();

        // Third result set is a single-row ack rollup: AcknowledgedCount + HasAcknowledged.
        // Both are nullable on the DTO; the SP returns NULL when the bulletin does not
        // require acknowledgement.
        var ack = await reader.ReadFirstOrDefaultAsync<AckRollup>();
        if (ack is not null)
        {
            header.AcknowledgedCount = ack.AcknowledgedCount;
            header.HasAcknowledged = ack.HasAcknowledged;
        }

        return header;
    }

    public async Task<bool> IsVisibleToUserAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var p = new
        {
            bulletinId,
            currentUserId = scope.CurrentUserId,
            isStaff = scope.IsStaff,
            isPupil = scope.IsPupil,
            isParent = scope.IsParent,
            canView = scope.CanView,
            canEdit = scope.CanEdit,
            canPin = scope.CanPin
        };

        var command = new CommandDefinition("[dbo].[usp_bulletin_is_visible]", p,
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        return await conn.ExecuteScalarAsync<bool>(command);
    }

    public async Task<PageResult<BulletinSummaryResponse>> GetSummariesAsync(BulletinVisibilityScope scope,
        FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Bulletins.GetBulletinSummaries.sql");

        var p = scope.ToSqlParams();

        return await GetListPagedAsync<BulletinSummaryResponse>(sql, p, filter, sort, paging, false, cancellationToken);
    }

    public async Task ReplaceAudiencesAsync(Guid bulletinId, IList<BulletinAudience> audiences,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        if (transaction is not null)
        {
            await ReplaceAudiencesCoreAsync(transaction.Connection!, transaction, bulletinId, audiences,
                cancellationToken);
            return;
        }

        // No caller-supplied transaction: open a local one so DELETE + INSERT are atomic.
        // Without this, a failure (or cancellation) after the DELETE would leave the
        // bulletin with no audience rows.
        using var conn = _factory.Create();
        conn.Open();
        using var localTx = conn.BeginTransaction();
        try
        {
            await ReplaceAudiencesCoreAsync(conn, localTx, bulletinId, audiences, cancellationToken);
            localTx.Commit();
        }
        catch
        {
            localTx.Rollback();
            throw;
        }
    }

    private static async Task ReplaceAudiencesCoreAsync(IDbConnection conn, IDbTransaction tx,
        Guid bulletinId, IList<BulletinAudience> audiences, CancellationToken cancellationToken)
    {
        const string deleteSql = "DELETE FROM dbo.BulletinAudiences WHERE BulletinId = @bulletinId;";
        await conn.ExecuteAsync(new CommandDefinition(deleteSql, new { bulletinId },
            transaction: tx, cancellationToken: cancellationToken));

        if (audiences.Count == 0)
        {
            return;
        }

        const string insertSql = @"
INSERT INTO dbo.BulletinAudiences (Id, BulletinId, AudienceKind, StudentGroupId)
VALUES (@Id, @BulletinId, @AudienceKind, @StudentGroupId);";

        var rows = audiences.Select(a => new
        {
            Id = a.Id == Guid.Empty ? Guid.NewGuid() : a.Id,
            BulletinId = bulletinId,
            AudienceKind = (byte)a.AudienceKind,
            a.StudentGroupId
        });

        await conn.ExecuteAsync(new CommandDefinition(insertSql, rows,
            transaction: tx, cancellationToken: cancellationToken));
    }

    private sealed class AckRollup
    {
        public int? AcknowledgedCount { get; set; }
        public bool? HasAcknowledged { get; set; }
    }
}
