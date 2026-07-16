using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStaffPreEmploymentChecksRepository : IEntityRepository<StaffPreEmploymentChecks>
{
    // 1:1 with the staff member — returns the single row, or null if never saved.
    Task<StaffPreEmploymentChecks?> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
