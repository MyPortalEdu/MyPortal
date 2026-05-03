using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class RolePermissionRepository : EntityRepository<RolePermission>, IRolePermissionRepository
    {
        public RolePermissionRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
            base(factory, authorizationService)
        {
        }

        public async Task<IList<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var sql = "[dbo].[sp_role_permission_get_by_role_id]";

            var p = new { roleId };

            using var conn = _factory.Create();

            return await conn.ExecuteStoredProcedureAsync<RolePermission>(sql, p, cancellationToken: cancellationToken);
        }
    }
}
