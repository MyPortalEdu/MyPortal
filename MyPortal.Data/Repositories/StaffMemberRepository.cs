using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;
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

public class StaffMemberRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffMember>(factory, authorizationService), IStaffMemberRepository
{
    public async Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("People.GetStaffMemberSummaries.sql");

        return await GetListPagedAsync<StaffMemberSummaryResponse>(sql, null, filter, sort, paging, false,
            cancellationToken);
    }

    public async Task<StaffMemberHeaderRow?> GetHeaderByIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var p = new { staffMemberId };

            var command = new CommandDefinition("[dbo].[usp_staff_member_get_header_by_id]", p, transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<StaffMemberHeaderRow>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<StaffBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var p = new { staffMemberId };

            var command = new CommandDefinition("[dbo].[usp_staff_member_get_basic_details_by_id]", p, transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<StaffBasicDetailsResponse>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<Guid?> GetStaffMemberIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<Guid?>(
                "[dbo].[usp_staff_member_get_id_by_person_id]", new { personId }, transaction,
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

    public async Task<IEnumerable<LookupResponse>> GetStaffLookupAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<LookupResponse>(
                "[dbo].[usp_staff_member_get_lookup]", null, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<PersonMatchResponse>> SearchPeopleForStaffCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var sql = SqlResourceLoader.Load("People.SearchPeopleForStaffCreate.sql");

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { like }, transaction,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<PersonMatchResponse>(command);

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

    public async Task<bool> IsManagedByAsync(Guid subjectStaffMemberId, Guid managerStaffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var p = new { subjectStaffMemberId, managerStaffMemberId };

            var command = new CommandDefinition("[dbo].[usp_staff_member_is_managed_by]", p, transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.ExecuteScalarAsync<bool>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeStaffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_staff_member_code_exists]",
                new { code, excludeStaffMemberId }, transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.ExecuteScalarAsync<bool>(command);
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
