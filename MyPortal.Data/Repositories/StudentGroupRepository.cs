using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class StudentGroupRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StudentGroup>(factory, authorizationService), IStudentGroupRepository
{
    public async Task<IList<StudentGroup>> GetStudentGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_student_group_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<StudentGroup>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task<PageResult<StudentGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Pastoral.GetStudentGroupSummaries.sql");

        return await GetListPagedAsync<StudentGroupSummaryResponse>(sql, new { academicYearId },
            filter, sort, paging, false, cancellationToken);
    }

    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_student_group_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task<bool> HasDownstreamDataAsync(Guid studentGroupId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<bool>(
                "[dbo].[usp_student_group_has_downstream_data_by_id]",
                new { studentGroupId }, transaction, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task<bool> CodeExistsAsync(Guid academicYearId, string code, Guid? excludeStudentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var result = await conn.ExecuteStoredProcedureAsync<bool>(
                "[dbo].[usp_student_group_code_exists]",
                new { academicYearId, code, excludeStudentGroupId }, transaction,
                cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
