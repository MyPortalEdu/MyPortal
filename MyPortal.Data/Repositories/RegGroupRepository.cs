using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class RegGroupRepository : BaseEntityRepository<RegGroup, Guid>, IRegGroupRepository
{
    public RegGroupRepository(IConnectionFactory factory) : base(factory)
    {
    }

    public async Task<IList<RegGroup>> GetRegGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();

        var sql = @"[dbo].[sp_reg_group_get_by_academic_year_id]";

        var param = new { academicYearId };

        var result = await conn.ExecuteStoredProcedureAsync<RegGroup>(sql, param,
            cancellationToken: cancellationToken);

        return result.ToList();
    }
}
