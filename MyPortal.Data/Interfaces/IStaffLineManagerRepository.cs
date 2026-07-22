using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffLineManagerRepository : IEntityRepository<StaffLineManager>
{
    Task<IEnumerable<StaffLineManagerRow>> GetHistoryAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task<StaffLineManager?> GetCurrentAsync(Guid staffMemberId, DateTime asOf,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
