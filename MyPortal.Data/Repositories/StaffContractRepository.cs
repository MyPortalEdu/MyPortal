using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StaffContractRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffContract>(factory, authorizationService), IStaffContractRepository
{
    public async Task<IEnumerable<StaffContract>> GetByEmploymentIdsAsync(IEnumerable<Guid> employmentIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = employmentIds.ToList();

        if (ids.Count == 0)
        {
            return Enumerable.Empty<StaffContract>();
        }

        // Full column list (incl. audit + version) so reconcile updates round-trip without
        // zeroing the created/audit columns; soft-deleted rows are excluded.
        const string sql =
            "SELECT [Id], [StaffEmploymentId], [ContractTypeId], [StaffRoleId], [ServiceTermId], " +
            "[DepartmentId], [PayScaleId], [PayScalePointId], [PostTitle], [StartDate], " +
            "[EndDate], [Fte], [HoursPerWeek], [WeeksPerYear], [AnnualSalary], [IsAgencySupply], " +
            "[SafeguardedSalary], [DailyRate], [IsDeleted], [CreatedById], [CreatedByIpAddress], " +
            "[CreatedAt], [LastModifiedById], [LastModifiedByIpAddress], [LastModifiedAt], [Version] " +
            "FROM [dbo].[StaffContracts] WHERE [StaffEmploymentId] IN @employmentIds AND [IsDeleted] = 0;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { employmentIds = ids }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<StaffContract>(command);
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
