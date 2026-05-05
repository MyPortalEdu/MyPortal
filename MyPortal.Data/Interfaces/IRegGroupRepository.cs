using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IRegGroupRepository : IBaseEntityRepository<RegGroup, Guid>
{
    Task<IList<RegGroup>> GetRegGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
