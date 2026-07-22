using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISenStatementRepository : IEntityRepository<SenStatement>
{
    Task<IEnumerable<SenStatement>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
