using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class DocumentRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : EntityRepository<Document>(factory, authorizationService), IDocumentRepository
    {
        public async Task<DocumentDetailsResponse> GetDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = "[dbo].[usp_document_get_details_by_id]";

            var param = new { documentId, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql, param,
                cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsByDirectoryId(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false)
        {
            using var conn = _factory.Create();

            var sql = "[dbo].[usp_document_get_details_by_directory]";

            var param = new
                { directoryId, includeDeleted, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql,
                param,
                cancellationToken: cancellationToken);

            return result.ToList();
        }

        public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsInSubtreeAsync(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false)
        {
            using var conn = _factory.Create();

            var sql = "[dbo].[usp_document_get_tree_by_directory_id]";

            var param = new
                { directoryId, includeDeleted, isStaff = AuthorizationService.GetCurrentUserType() == UserType.Staff };

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql, param,
                cancellationToken: cancellationToken);

            return result.ToList();
        }
    }
}
