using System.Data;
using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Data.Interfaces;

namespace MyPortal.Data.Repositories;

public class BulletinAcknowledgementRepository : IBulletinAcknowledgementRepository
{
    private readonly IDbConnectionFactory _factory;

    public BulletinAcknowledgementRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<bool> AcknowledgeAsync(Guid bulletinId, Guid userId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        // Idempotent insert: the unique key on (BulletinId, UserId) means re-acks are
        // effectively no-ops. We use NOT EXISTS rather than swallowing a unique-violation
        // exception so the happy path stays exception-free under contention.
        const string sql = @"
INSERT INTO dbo.BulletinAcknowledgements (Id, BulletinId, UserId, AcknowledgedAt)
SELECT NEWID(), @bulletinId, @userId, SYSUTCDATETIME()
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.BulletinAcknowledgements
    WHERE BulletinId = @bulletinId AND UserId = @userId
);";

        var conn = transaction?.Connection ?? _factory.Create();
        var owns = transaction is null;
        try
        {
            var rows = await conn.ExecuteAsync(new CommandDefinition(sql,
                new { bulletinId, userId },
                transaction: transaction,
                cancellationToken: cancellationToken));
            return rows == 1;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
