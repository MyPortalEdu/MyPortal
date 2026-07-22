using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentSenNeedRepository : IEntityRepository<StudentSenNeed>
{
    Task<IEnumerable<StudentSenNeed>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
