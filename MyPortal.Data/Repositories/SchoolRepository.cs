using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class SchoolRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<School>(factory,
        authorizationService), ISchoolRepository
{
    public async Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_school_get_details_local]";
        var result =
            await conn.ExecuteStoredProcedureAsync<SchoolDetailsResponse>(sql, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<SchoolDetailsResponse?> GetDetailsByIdAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_school_get_details_by_id]";
        var p = new { schoolId };
        var result =
            await conn.ExecuteStoredProcedureAsync<SchoolDetailsResponse>(sql, p, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }

    public async Task<Guid?> GetLocalSchoolPayZoneIdAsync(CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[usp_school_get_local_pay_zone_id]";
        var result =
            await conn.ExecuteStoredProcedureAsync<Guid?>(sql, cancellationToken: cancellationToken);

        return result.FirstOrDefault();
    }
}