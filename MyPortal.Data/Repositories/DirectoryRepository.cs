using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Data.Repositories
{
    public class DirectoryRepository : EntityRepository<Directory>, IDirectoryRepository
    {
        public DirectoryRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<DirectoryDetailsResponse?> GetDetailsByIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_details_by_id]";

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, new { directoryId },
                cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoriesByParentIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_details_by_parent_id]";

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, new { directoryId },
                cancellationToken: cancellationToken);

            return result.ToList();
        }

        public async Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoryTreeAsync(Guid directoryId,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_directory_get_tree_by_id]";

            var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, new { directoryId },
                cancellationToken: cancellationToken);

            return result.ToList();
        }
    }
}
