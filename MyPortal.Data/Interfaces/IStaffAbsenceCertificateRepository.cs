using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffAbsenceCertificateRepository : IEntityRepository<StaffAbsenceCertificate>
{
    /// <summary>
    /// Certificates for the given absences (batched so the area loads in one round trip).
    /// </summary>
    Task<IEnumerable<StaffAbsenceCertificate>> GetByAbsenceIdsAsync(IEnumerable<Guid> absenceIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
