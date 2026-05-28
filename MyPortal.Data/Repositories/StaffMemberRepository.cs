using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
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

    public async Task<StaffMemberDetailsResponse?> GetDetailsByIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var p = new { staffMemberId };

            var command = new CommandDefinition("[dbo].[usp_staff_member_get_details_by_id]", p,
                transaction: transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await using var reader = await conn.QueryMultipleAsync(command);

            var header = await reader.ReadFirstOrDefaultAsync<StaffMemberDetailsResponse>();

            if (header == null)
            {
                return null;
            }

            header.Person = await reader.ReadFirstAsync<PersonDetailsResponse>();

            return header;
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
