using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentPepRepository : IEntityRepository<StudentPep>
{
    Task<IEnumerable<StudentPep>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
