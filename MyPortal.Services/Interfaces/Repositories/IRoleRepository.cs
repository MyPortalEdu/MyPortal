using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IRoleRepository : IEntityRepository<Role>
    {
        Task<RoleDetailsDto> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);
    }
}
