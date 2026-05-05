using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IYearGroupRepository : IBaseEntityRepository<YearGroup, Guid>
{
    Task<IList<YearGroup>> GetYearGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
