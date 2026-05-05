using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class YearGroupRepository : BaseEntityRepository<YearGroup, Guid>, IYearGroupRepository
{
    public YearGroupRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task<IList<YearGroup>> GetYearGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_year_group_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<YearGroup>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
