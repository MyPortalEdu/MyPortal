using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ISenProvisionRepository : IEntityRepository<SenProvision>
{
    Task<IEnumerable<SenProvision>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
