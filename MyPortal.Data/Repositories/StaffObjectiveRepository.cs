using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffObjectiveRepository : EntityRepository<StaffObjective>, IStaffObjectiveRepository
{
    public StaffObjectiveRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }

    public async Task<IEnumerable<StaffObjective>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // Full column list (incl. audit + version) so reconcile updates round-trip without zeroing
        // the created/audit columns; soft-deleted rows excluded.
        const string sql =
            "SELECT [Id], [StaffMemberId], [ReviewId], [CategoryId], [Title], [Description], [SuccessCriteria], " +
            "[DueDate], [StatusId], [ProgressNotes], [IsDeleted], [CreatedById], [CreatedByIpAddress], [CreatedAt], " +
            "[LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version] FROM [dbo].[StaffObjectives] " +
            "WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<StaffObjective>(command);
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
