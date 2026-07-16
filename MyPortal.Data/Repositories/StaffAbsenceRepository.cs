using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffAbsenceRepository : EntityRepository<StaffAbsence>, IStaffAbsenceRepository
{
    public StaffAbsenceRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }

    public async Task<IEnumerable<StaffAbsence>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // StaffAbsences carries no audit/soft-delete columns — a lean record table; rows are
        // hard-deleted on reconcile.
        const string sql =
            "SELECT [Id], [StaffMemberId], [AbsenceTypeId], [IllnessTypeId], [StartDate], [EndDate], " +
            "[IsConfidential], [Notes] FROM [dbo].[StaffAbsences] WHERE [StaffMemberId] = @staffMemberId;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<StaffAbsence>(command);
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
