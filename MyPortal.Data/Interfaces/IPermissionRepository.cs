using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces
{
    public interface IPermissionRepository : IEntityReadRepository<Permission>
    {
        Task<IList<PermissionResponse>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
