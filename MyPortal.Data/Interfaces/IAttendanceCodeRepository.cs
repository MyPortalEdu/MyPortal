using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IAttendanceCodeRepository : IEntityRepository<AttendanceCode>
{
    Task<IList<AttendanceCode>> GetByIdsAsync(IEnumerable<Guid> attendanceCodeIds,
        CancellationToken cancellationToken);
}
