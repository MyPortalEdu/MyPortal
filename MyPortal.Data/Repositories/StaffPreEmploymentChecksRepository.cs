using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffPreEmploymentChecksRepository : EntityRepository<StaffPreEmploymentChecks>,
    IStaffPreEmploymentChecksRepository
{
    public StaffPreEmploymentChecksRepository(IDbConnectionFactory factory,
        IAuthorizationService authorizationService) : base(factory, authorizationService)
    {
    }

    public async Task<StaffPreEmploymentChecks?> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // Full column list (incl. audit + version) so an update round-trips without zeroing
        // the created/audit columns; soft-deleted rows are excluded. 1:1, so single-or-null.
        const string sql =
            "SELECT [Id], [StaffMemberId], [IdentityCheckedDate], [ProhibitionFromTeachingCheckedDate], " +
            "[ProhibitionFromManagementCheckedDate], [ChildcareDisqualificationCheckedDate], " +
            "[MedicalFitnessCheckedDate], [QualificationsVerifiedDate], [Notes], [IsDeleted], [CreatedById], " +
            "[CreatedByIpAddress], [CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], " +
            "[Version] FROM [dbo].[StaffPreEmploymentChecks] WHERE [StaffMemberId] = @staffMemberId " +
            "AND [IsDeleted] = 0;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<StaffPreEmploymentChecks>(command);
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
