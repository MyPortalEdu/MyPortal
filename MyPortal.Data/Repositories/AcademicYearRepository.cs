using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class AcademicYearRepository : EntityRepository<AcademicYear>, IAcademicYearRepository
{
    public AcademicYearRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
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

    public async Task<IList<AcademicYearSummaryResponse>> GetSummariesAsync(
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var rows = await conn.ExecuteStoredProcedureAsync<AcademicYearSummaryResponse>(
            "[dbo].[sp_academic_year_get_summaries]", parameters: null,
            cancellationToken: cancellationToken);

        return rows.ToList();
    }

    public async Task<AcademicYearDetailsResult?> GetDetailsByIdAsync(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition("[dbo].[sp_academic_year_get_details_by_id]",
            new { academicYearId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<AcademicYearDetailsResponse>();
        if (header is null)
        {
            return null;
        }

        // Order matches the SP: header, terms, holidays, periods.
        var terms     = (await reader.ReadAsync<AcademicTermResponse>()).ToList();
        var holidays  = (await reader.ReadAsync<SchoolHolidayRow>()).ToList();
        var periods   = (await reader.ReadAsync<AttendancePeriodResponse>()).ToList();

        return new AcademicYearDetailsResult
        {
            Header = header,
            Terms = terms,
            Holidays = holidays,
            AttendancePeriods = periods
        };
    }

    public async Task<AcademicYearSummaryResponse?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var rows = await conn.ExecuteStoredProcedureAsync<AcademicYearSummaryResponse>(
            "[dbo].[sp_academic_year_get_current]", parameters: null,
            cancellationToken: cancellationToken);

        return rows.FirstOrDefault();
    }
}
