using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentCareEpisodeRepository : IEntityRepository<StudentCareEpisode>
{
    Task<IEnumerable<StudentCareEpisode>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
