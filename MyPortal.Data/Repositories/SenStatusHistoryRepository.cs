using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class SenStatusHistoryRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<SenStatusHistory>(factory, authorizationService), ISenStatusHistoryRepository
{
    public async Task<IEnumerable<SenStatusHistory>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<SenStatusHistory>(
                "[dbo].[usp_sen_status_history_get_by_student_id]", new { studentId }, transaction,
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
