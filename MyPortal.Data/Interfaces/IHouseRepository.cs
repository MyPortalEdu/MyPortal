using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IHouseRepository : IEntityRepository<House>
{
    Task<IList<House>> GetHousesByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
