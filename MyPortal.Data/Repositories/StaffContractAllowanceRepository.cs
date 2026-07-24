using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffContractAllowanceRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffContractAllowance>(factory, authorizationService), IStaffContractAllowanceRepository
{
    public async Task<IEnumerable<StaffContractAllowance>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = contractIds.ToList();

        if (ids.Count == 0)
        {
            return Enumerable.Empty<StaffContractAllowance>();
        }

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffContractAllowance>(
                "[dbo].[usp_staff_contract_allowance_get_by_contract_ids]",
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
