using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class StaffMemberRepository : EntityRepository<StaffMember>, IStaffMemberRepository
{
    public StaffMemberRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }

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

    public async Task<Guid?> GetStaffMemberIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT TOP 1 [Id] FROM [dbo].[StaffMembers] WHERE [PersonId] = @personId AND [IsDeleted] = 0;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { personId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.ExecuteScalarAsync<Guid?>(command);
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
}
