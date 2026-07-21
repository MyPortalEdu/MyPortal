using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class StudentRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Student>(factory, authorizationService), IStudentRepository
{
    public async Task<PageResult<StudentSummaryResponse>> GetStudentsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("People.GetStudentSummaries.sql");

        return await GetListPagedAsync<StudentSummaryResponse>(sql, null, filter, sort, paging, false,
            cancellationToken);
    }

    public async Task<StudentHeaderRow?> GetHeaderByIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_get_header_by_id]", new { studentId },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<StudentHeaderRow>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<StudentBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_get_basic_details_by_id]", new { studentId },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<StudentBasicDetailsResponse>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<Guid?> GetStudentIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<Guid?>(
                "[dbo].[usp_student_get_id_by_person_id]", new { personId }, transaction,
                cancellationToken: cancellationToken);

            return rows.FirstOrDefault();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<int> GetNextAdmissionNumberAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_get_next_admission_number]", null,
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.ExecuteScalarAsync<int>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<int> GetMaxUpnSerialAsync(string prefix9, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_get_max_upn_serial]", new { prefix9 },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.ExecuteScalarAsync<int>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<StudentMatchResponse>> SearchPeopleForStudentCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var sql = SqlResourceLoader.Load("People.SearchPeopleForStudentCreate.sql");

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { like }, transaction,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<StudentMatchResponse>(command);

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
}
