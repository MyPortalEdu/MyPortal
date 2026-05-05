using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IAttendancePeriodRepository : IBaseEntityRepository<AttendancePeriod, Guid>
{
    Task<IList<AttendancePeriod>> GetAttendancePeriodsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}