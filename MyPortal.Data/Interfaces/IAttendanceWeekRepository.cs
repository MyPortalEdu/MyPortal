using System.Data;
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IAttendanceWeekRepository : IBaseEntityRepository<AttendanceWeek, Guid>
{
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
