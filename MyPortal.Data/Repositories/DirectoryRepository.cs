using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Data.Repositories
{
    public class DirectoryRepository : EntityRepository<Directory>, IDirectoryRepository
    {
        public DirectoryRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
            factory, authorizationService)
        {
        }

        public async Task<DirectoryDetailsResponse?> GetDetailsByIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_details_by_id]";

            var param = new
                { directoryId, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, param,
                cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoriesByParentIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_details_by_parent_id]";

            var param = new
                { directoryId, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, param,
                cancellationToken: cancellationToken);

            return result.ToList();
        }

        public async Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_tree_by_id]";

            var param = new
                { directoryId, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, param,
                cancellationToken: cancellationToken);

            return result.ToList();
        }

        public async Task<bool> IsInSubtreeAsync(Guid rootDirectoryId, Guid candidateDirectoryId,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_is_in_subtree]";

            var param = new { rootDirectoryId, candidateDirectoryId };

            var result = await conn.ExecuteScalarAsync<bool>(sql, param, commandType:CommandType.StoredProcedure);

            return result;
        }
    }
}
