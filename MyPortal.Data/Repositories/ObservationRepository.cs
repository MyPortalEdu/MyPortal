using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class ObservationRepository : EntityRepository<Observation>, IObservationRepository
{
    public ObservationRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }

    public async Task<IEnumerable<Observation>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // Observations is a lean record table (no audit / soft-delete); the staff member is the
        // observee. Rows are hard-deleted on reconcile.
        const string sql =
            "SELECT [Id], [Date], [ObserveeId], [ObserverId], [OutcomeId], [Focus], [SubjectObserved], " +
            "[Strengths], [AreasForDevelopment], [Notes] FROM [dbo].[Observations] " +
            "WHERE [ObserveeId] = @staffMemberId;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<Observation>(command);
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
