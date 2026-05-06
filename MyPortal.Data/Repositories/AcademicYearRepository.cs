using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class AcademicYearRepository : BaseEntityRepository<AcademicYear, Guid>, IAcademicYearRepository
{
    public AcademicYearRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task<DateTime?> GetEarliestTermStartDateAsync(Guid academicYearId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<DateTime?>(
                "[dbo].[sp_academic_year_get_earliest_term_start_date_by_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task<bool> HasDownstreamDataAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<bool>(
                "[dbo].[sp_academic_year_has_downstream_data_by_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task<bool> HasOverlapAsync(Guid? excludeAcademicYearId, DateTime rangeStart, DateTime rangeEnd,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<bool>(
                "[dbo].[sp_academic_year_check_overlap]",
                new { excludeAcademicYearId, rangeStart, rangeEnd },
                transaction, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
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
