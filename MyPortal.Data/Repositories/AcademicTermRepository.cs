using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories;

public class AcademicTermRepository : BaseEntityRepository<AcademicTerm, Guid>, IAcademicTermRepository
{
    protected AcademicTermRepository(IConnectionFactory factory) : base(factory)
    {
    }
}