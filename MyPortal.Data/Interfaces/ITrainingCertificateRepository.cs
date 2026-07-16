using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ITrainingCertificateRepository : IEntityRepository<TrainingCertificate>
{
    Task<IEnumerable<TrainingCertificate>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
