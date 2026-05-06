using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class AcademicTermRepository : BaseEntityRepository<AcademicTerm, Guid>, IAcademicTermRepository
{
    public AcademicTermRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            // The SP returns no result set; pick a discardable element type.
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[sp_academic_term_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    private (IDbConnection conn, bool owns) AcquireConnection(IDbTransaction? transaction)
    {
        if (transaction?.Connection is { } shared)
        {
            return (shared, false);
        }

        return (_factory.Create(), true);
    }
}
