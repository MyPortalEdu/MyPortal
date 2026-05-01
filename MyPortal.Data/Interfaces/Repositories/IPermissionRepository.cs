using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Repositories.Base;

namespace MyPortal.Data.Interfaces.Repositories
{
    public interface IPermissionRepository : IEntityReadRepository<Permission>
    {
        Task<IList<PermissionResponse>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
