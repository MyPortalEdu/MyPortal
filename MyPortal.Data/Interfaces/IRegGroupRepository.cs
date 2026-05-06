using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IRegGroupRepository : IEntityRepository<RegGroup>
{
    Task<IList<RegGroup>> GetRegGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
