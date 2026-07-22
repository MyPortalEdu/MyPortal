using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffLineManagerRepository : IEntityRepository<StaffLineManager>
{
    /// <summary>The staff member's reporting-line history, newest first, with manager names resolved.</summary>
    Task<IEnumerable<StaffLineManagerRow>> GetHistoryAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>The row current on <paramref name="asOf"/>, or null if unmanaged that day.</summary>
    Task<StaffLineManager?> GetCurrentAsync(Guid staffMemberId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
