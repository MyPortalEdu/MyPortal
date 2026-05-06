using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

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

        var sql = @"[dbo].[sp_house_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<House>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
