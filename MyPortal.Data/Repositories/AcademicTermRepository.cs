using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class AcademicTermRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : Base.EntityRepository<AcademicTerm>(factory, authorizationService), IAcademicTermRepository
{
    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            // The SP returns no result set; pick a discardable element type.
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_academic_term_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
