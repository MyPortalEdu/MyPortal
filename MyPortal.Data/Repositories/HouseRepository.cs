using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class HouseRepository : BaseEntityRepository<House, Guid>, IHouseRepository
{
    public HouseRepository(IConnectionFactory factory) : base(factory)
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
