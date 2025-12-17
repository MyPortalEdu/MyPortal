using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IPermissionRepository : IEntityReadRepository<Permission>
    {
        Task<IList<PermissionResponse>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
