using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IScheduledIncrementRepository : IEntityRepository<ScheduledIncrement>
{
    /// <summary>
    /// Scheduled increments with their term identity. <paramref name="serviceTermId"/> null returns
    /// all terms; <paramref name="scheduledOnly"/> limits to still-pending runs; <paramref name="dueBy"/>
    /// (non-null) limits to those effective on or before it.
    /// </summary>
    Task<IReadOnlyList<ScheduledIncrementRow>> GetScheduledAsync(Guid? serviceTermId, bool scheduledOnly,
        DateTime? dueBy, CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
