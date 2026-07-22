using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractSalaryChangeRepository : IEntityRepository<StaffContractSalaryChange>
{
    /// <summary>Salary-change history for the given contracts, newest first, with the changing
    /// user's display name resolved.</summary>
    Task<IEnumerable<StaffContractSalaryChangeRow>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
