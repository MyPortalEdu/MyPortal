using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IStaffContractSalaryChangeRepository : IEntityRepository<StaffContractSalaryChange>
{
    Task<IEnumerable<StaffContractSalaryChangeRow>> GetByContractIdsAsync(IEnumerable<Guid> contractIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
