using System.Data;
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IAcademicTermRepository : IBaseEntityRepository<AcademicTerm, Guid>
{
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
