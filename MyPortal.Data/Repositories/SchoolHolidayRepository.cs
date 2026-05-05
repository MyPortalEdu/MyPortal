using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class SchoolHolidayRepository : BaseEntityRepository<SchoolHoliday, Guid>, ISchoolHolidayRepository
{
    protected SchoolHolidayRepository(IConnectionFactory factory) : base(factory)
    {
    }
}