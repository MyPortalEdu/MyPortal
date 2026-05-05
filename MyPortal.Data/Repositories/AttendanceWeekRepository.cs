using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class AttendanceWeekRepository : BaseEntityRepository<AttendanceWeek, Guid>, IAttendanceWeekRepository
{
    public AttendanceWeekRepository(IConnectionFactory factory) : base(factory)
    {
    }
}