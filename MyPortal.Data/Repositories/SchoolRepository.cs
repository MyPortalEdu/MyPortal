using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Schools.Queries;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class SchoolRepository : EntityRepository<School>, ISchoolRepository
{
    public SchoolRepository(IDbConnectionFactory factory) : base(factory)
    {
    }

    public async Task<SchoolDetailsDto?> GetLocalSchoolAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_school_get_local]";
        var result =
            await conn.ExecuteStoredProcedureAsync<SchoolDetailsDto>(sql, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<SchoolDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_school_get_details_by_id]";
        var result = await conn.ExecuteStoredProcedureAsync<SchoolDetailsDto>(sql, cancellationToken: cancellationToken);
        
        return result.FirstOrDefault();
    }
}