using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IRolePermissionRepository : IEntityRepository<RolePermission>
    {
        Task<IList<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
