using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentSenNeedRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StudentSenNeed>(factory, authorizationService), IStudentSenNeedRepository
{
    public async Task<IEnumerable<StudentSenNeed>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StudentSenNeed>(
                "[dbo].[usp_student_sen_need_get_by_student_id]", new { studentId }, transaction,
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
