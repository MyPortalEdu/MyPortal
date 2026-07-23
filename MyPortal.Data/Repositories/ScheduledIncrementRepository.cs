using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;

namespace MyPortal.Data.Repositories;

public class ScheduledIncrementRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<ScheduledIncrement>(factory, authorizationService), IScheduledIncrementRepository
{
    public async Task<IReadOnlyList<ScheduledIncrementRow>> GetScheduledAsync(Guid? serviceTermId,
        bool scheduledOnly, DateTime? dueBy, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var sql = SqlResourceLoader.Load("People.GetScheduledIncrements.sql");

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql,
                new
                {
                    serviceTermId,
                    statusScheduledOnly = scheduledOnly ? 1 : 0,
                    dueBy = dueBy?.Date
                },
                transaction, cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<ScheduledIncrementRow>(command);
            return rows.AsList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }
}
