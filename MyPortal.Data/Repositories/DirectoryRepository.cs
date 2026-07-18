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
    public class DirectoryRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : EntityRepository<Directory>(factory, authorizationService), IDirectoryRepository
    {
        public async Task<DirectoryDetailsResponse?> GetDetailsByIdAsync(Guid directoryId, CancellationToken cancellationToken, IDbTransaction? transaction = null)
        {
            // When a transaction is passed in, we MUST use its own connection — a SqlTransaction
            // is bound to the connection it was opened on and can't be reused on a fresh one
            // (throws "transaction is not associated with the connection"). AcquireConnection
            // shares the caller's connection in that case and only mints a new one otherwise.
            var (conn, owns) = AcquireConnection(transaction);
            try
            {
                var sql = @"[dbo].[usp_directory_get_details_by_id]";

                var param = new
                    { directoryId, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

                var result = await conn.ExecuteStoredProcedureAsync<DirectoryDetailsResponse>(sql, param,
                    cancellationToken: cancellationToken, transaction: transaction);

                return result.FirstOrDefault();
            }
            finally
            {
                if (owns) conn.Dispose();
            }
        }

        public async Task<IReadOnlyList<DirectoryDetailsResponse>> GetDirectoriesByParentIdAsync(Guid directoryId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[usp_directory_get_details_by_parent_id]";

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

            var sql = @"[dbo].[usp_directory_get_tree_by_id]";

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

            var sql = @"[dbo].[usp_directory_is_in_subtree]";

            var param = new { rootDirectoryId, candidateDirectoryId };

            var result = await conn.ExecuteScalarAsync<bool>(sql, param, commandType:CommandType.StoredProcedure);

            return result;
        }

        public async Task<DirectoryOwnerReference?> GetReferencingOwnerAsync(Guid directoryId,
            CancellationToken cancellationToken, IDbTransaction? transaction = null)
        {
            var (conn, owns) = AcquireConnection(transaction);
            try
            {
                var sql = @"[dbo].[usp_directory_get_referencing_owner]";
                var param = new { directoryId };

                var result = await conn.ExecuteStoredProcedureAsync<DirectoryOwnerReference>(sql, param,
                    cancellationToken: cancellationToken, transaction: transaction);

                return result.FirstOrDefault();
            }
            finally
            {
                if (owns) conn.Dispose();
            }
        }
    }
}
