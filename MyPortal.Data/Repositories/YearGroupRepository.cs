using System.Data;
using Dapper;
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

namespace MyPortal.Data.Repositories;

public class YearGroupRepository : EntityRepository<YearGroup>, IYearGroupRepository
{
    public YearGroupRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<YearGroup>> GetYearGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var result = await conn.ExecuteStoredProcedureAsync<YearGroup>(
            "[dbo].[sp_year_group_get_by_academic_year_id]", new { academicYearId },
            cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task<PageResult<YearGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Pastoral.GetYearGroupSummaries.sql");

        return await GetListPagedAsync<YearGroupSummaryResponse>(sql, new { academicYearId },
            filter, sort, paging, false, cancellationToken);
    }

    public async Task<YearGroupDetailsResult?> GetDetailsByIdAsync(Guid yearGroupId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition("[dbo].[sp_year_group_get_details_by_id]",
            new { yearGroupId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<YearGroupDetailsResponse>();
        if (header is null)
        {
            return null;
        }

        var supervisors = (await reader.ReadAsync<YearGroupSupervisorResponse>()).ToList();

        return new YearGroupDetailsResult
        {
            Header = header,
            Supervisors = supervisors
        };
    }
}
