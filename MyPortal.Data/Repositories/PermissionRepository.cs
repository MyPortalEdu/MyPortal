using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class PermissionRepository : EntityReadRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(IDbConnectionFactory factory) : base(factory)
        {
            
        }

        public async Task<IList<PermissionResponse>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_permission_get_by_user_id]";

            var result = await conn.ExecuteStoredProcedureAsync<PermissionResponse>(sql, new { userId }, cancellationToken: cancellationToken);
            return result;
        }
    }
}
