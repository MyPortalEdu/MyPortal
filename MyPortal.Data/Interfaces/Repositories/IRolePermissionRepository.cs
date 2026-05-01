using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Repositories.Base;

namespace MyPortal.Data.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IEntityRepository<RolePermission>
    {
        Task<IList<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
