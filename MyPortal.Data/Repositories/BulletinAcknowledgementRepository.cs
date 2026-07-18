using System.Data;
using MyPortal.Common.Interfaces;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class BulletinAcknowledgementRepository(IDbConnectionFactory factory) : IBulletinAcknowledgementRepository
{
    public async Task<bool> AcknowledgeAsync(Guid bulletinId, Guid userId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var conn = transaction?.Connection ?? factory.Create();
        var owns = transaction is null;
        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_bulletin_acknowledgement_add]",
                new { bulletinId, userId }, transaction, cancellationToken: cancellationToken);
            return rows.FirstOrDefault() == 1;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
