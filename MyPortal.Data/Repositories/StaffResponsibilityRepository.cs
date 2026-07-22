using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffResponsibilityRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffResponsibility>(factory, authorizationService), IStaffResponsibilityRepository
{
    public async Task<IEnumerable<StaffResponsibility>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // Full column list (incl. audit + version) so reconcile updates round-trip without zeroing
        // the created/audit columns; soft-deleted rows are excluded.
        const string sql =
            "SELECT [Id], [StaffMemberId], [ResponsibilityTypeId], [StartDate], [EndDate], [Notes], " +
            "[IsDeleted], [CreatedById], [CreatedByIpAddress], [CreatedAt], [LastModifiedById], " +
            "[LastModifiedByIpAddress], [LastModifiedAt], [Version] FROM [dbo].[StaffResponsibilities] " +
            "WHERE [StaffMemberId] = @staffMemberId AND [IsDeleted] = 0;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<StaffResponsibility>(command);
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
