using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces
{
    public interface IRolePermissionRepository : IEntityRepository<RolePermission>
    {
        Task<IList<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
