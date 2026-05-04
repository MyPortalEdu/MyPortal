using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class AcademicYearRepository : BaseEntityRepository<AcademicYear, Guid>, IAcademicYearRepository
{
    protected AcademicYearRepository(IConnectionFactory factory) : base(factory)
    {
    }
}