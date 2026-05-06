using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IAttendancePeriodRepository : IEntityRepository<AttendancePeriod>
{
    Task<IList<AttendancePeriod>> GetAttendancePeriodsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
