using System.Data;
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IAttendancePeriodRepository : IBaseEntityRepository<AttendancePeriod, Guid>
{
    Task<IList<AttendancePeriod>> GetAttendancePeriodsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
