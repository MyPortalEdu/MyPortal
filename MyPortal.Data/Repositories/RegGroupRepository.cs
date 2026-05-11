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

public class RegGroupRepository : EntityRepository<RegGroup>, IRegGroupRepository
{
    public RegGroupRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<RegGroup>> GetRegGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_reg_group_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<RegGroup>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
    
    public async Task<PageResult<RegGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId, FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null, CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Pastoral.GetRegGroupSummaries.sql");

        return await GetListPagedAsync<RegGroupSummaryResponse>(sql, new { academicYearId },
            filter, sort, paging, false, cancellationToken);
    }
    
    public async Task<RegGroupDetailsResponse?> GetDetailsByIdAsync(Guid regGroupId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition("[dbo].[usp_reg_group_get_details_by_id]",
            new { regGroupId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<RegGroupDetailsResponse>();
        if (header is null)
        {
            return null;
        }

        header.Supervisors = (await reader.ReadAsync<StudentGroupSupervisorResponse>()).ToList();
        return header;
    }
}
