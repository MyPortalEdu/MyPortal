using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IHouseRepository : IBaseEntityRepository<House, Guid>
{
    Task<IList<House>> GetHousesByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
