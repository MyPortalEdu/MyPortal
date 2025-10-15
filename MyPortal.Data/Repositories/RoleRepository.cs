using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class RoleRepository : EntityRepository<Role>, IRoleRepository
    {
        public RoleRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<RoleDetailsDto?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();
            
            var sql = "[dbo].[sp_role_get_details_by_id]";

            var p = new { roleId };
            
            var result = await conn.ExecuteStoredProcedureAsync<RoleDetailsDto>(sql, p, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }
    }
}
