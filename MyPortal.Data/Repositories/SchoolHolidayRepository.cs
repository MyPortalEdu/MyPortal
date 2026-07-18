using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class SchoolHolidayRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<SchoolHoliday>(factory, authorizationService), ISchoolHolidayRepository
{
    public async Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_school_holiday_delete_by_academic_year_id]",
                new { academicYearId }, transaction, cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }
}
