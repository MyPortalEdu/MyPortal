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

public class HouseRepository : EntityRepository<House>, IHouseRepository
{
    public HouseRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<House>> GetHousesByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var result = await conn.ExecuteStoredProcedureAsync<House>(
            "[dbo].[usp_house_get_by_academic_year_id]", new { academicYearId },
            cancellationToken: cancellationToken);

        return result.ToList();
    }

    public async Task<PageResult<HouseSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("Pastoral.GetHouseSummaries.sql");

        return await GetListPagedAsync<HouseSummaryResponse>(sql, new { academicYearId },
            filter, sort, paging, false, cancellationToken);
    }

    public async Task<HouseDetailsResult?> GetDetailsByIdAsync(Guid houseId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var command = new CommandDefinition("[dbo].[usp_house_get_details_by_id]",
            new { houseId },
            commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        using var reader = await conn.QueryMultipleAsync(command);

        var header = await reader.ReadFirstOrDefaultAsync<HouseDetailsResponse>();
        if (header is null)
        {
            return null;
        }

        var supervisors = (await reader.ReadAsync<StudentGroupSupervisorResponse>()).ToList();

        return new HouseDetailsResult
        {
            Header = header,
            Supervisors = supervisors
        };
    }
}
