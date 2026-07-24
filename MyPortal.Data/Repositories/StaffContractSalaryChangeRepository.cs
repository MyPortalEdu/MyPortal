using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffContractSalaryChangeRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffContractSalaryChange>(factory, authorizationService), IStaffContractSalaryChangeRepository
{
    public async Task<IReadOnlyList<Guid>> GetIncrementedContractIdsAsync(IEnumerable<Guid> contractIds,
        DateTime effectiveDate, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = contractIds.ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        const string sql =
            "SELECT DISTINCT [StaffContractId] FROM [dbo].[StaffContractSalaryChanges] " +
            "WHERE [Source] = 'Increment' AND [EffectiveDate] = @effectiveDate " +
            "AND [StaffContractId] IN @ids;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { effectiveDate = effectiveDate.Date, ids },
                transaction, cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<Guid>(command);
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

    public async Task<IEnumerable<StaffContractSalaryChangeRow>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = contractIds.ToList();

        if (ids.Count == 0)
        {
            return Enumerable.Empty<StaffContractSalaryChangeRow>();
        }

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffContractSalaryChangeRow>(
                "[dbo].[usp_staff_contract_salary_change_get_by_contract_ids]",
                new { contractIds = ids.ToGuidTvp() }, transaction,
                cancellationToken: cancellationToken);
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
