using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IYearGroupRepository : IEntityRepository<YearGroup>
{
    Task<IList<YearGroup>> GetYearGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
