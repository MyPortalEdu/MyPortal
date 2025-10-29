using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class RolePermissionRepository : EntityRepository<RolePermission>, IRolePermissionRepository
    {
        protected RolePermissionRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
            base(factory, authorizationService)
        {
        }

        public async Task<IList<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var sql = "[dbo].[sp_role_permission_get_by_role_id]";

            var p = new { roleId };

            var conn = _factory.Create();

            return await conn.ExecuteStoredProcedureAsync<RolePermission>(sql, p, cancellationToken: cancellationToken);
        }
    }
}
