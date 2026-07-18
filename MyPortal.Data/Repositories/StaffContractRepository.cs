using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

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

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffContract>(
                "[dbo].[usp_staff_contract_get_by_employment_ids]",
                new { employmentIds = ids.ToGuidTvp() }, transaction,
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
