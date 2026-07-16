using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IObservationRepository : IEntityRepository<Observation>
{
    /// <summary>Observations where the staff member is the observee.</summary>
    Task<IEnumerable<Observation>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
