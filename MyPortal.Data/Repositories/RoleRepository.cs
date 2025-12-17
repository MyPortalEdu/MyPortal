using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories
{
    public class RoleRepository : EntityRepository<Role>, IRoleRepository
    {
        public RoleRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();
            
            var sql = "[dbo].[sp_role_get_details_by_id]";

            var p = new { roleId };
            
            var result = await conn.ExecuteStoredProcedureAsync<RoleDetailsResponse>(sql, p, cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
            CancellationToken cancellationToken = default)
        {
            var sql = SqlResourceLoader.Load("System.Roles.GetRoleSummaries.sql");

            var result =
                await GetListPagedAsync<RoleSummaryResponse>(sql, null, filter, sort, paging, false, cancellationToken);

            return result;
        }
    }
}
