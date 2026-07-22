using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentChildProtectionPlanRepository : IEntityRepository<StudentChildProtectionPlan>
{
    Task<IEnumerable<StudentChildProtectionPlan>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
