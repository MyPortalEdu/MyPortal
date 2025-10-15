using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;

namespace MyPortal.Data.Repositories
{
    public class RoleRepository : EntityRepository<Role>, IRoleRepository
    {
        public RoleRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<RoleDetailsDto> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
