using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Interfaces.Repositories;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories
{
    public class DocumentRepository : EntityRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(IDbConnectionFactory factory) : base(factory)
        {
        }

        public async Task<DocumentDetailsResponse?> GetDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_document_get_details_by_id]";

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql, new { documentId },
                cancellationToken: cancellationToken);

            return result.FirstOrDefault();
        }

        public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsByDirectoryId(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_document_get_details_by_directory]";

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql,
                new { directoryId, includeDeleted },
                cancellationToken: cancellationToken);

            return result.ToList();
        }

        public async Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsInSubtreeAsync(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false)
        {
            using var conn = _factory.Create();

            var sql = @"[dbo].[sp_document_get_tree_by_directory_id]";

            var result = await conn.ExecuteStoredProcedureAsync<DocumentDetailsResponse>(sql, new { directoryId, includeDeleted },
                cancellationToken: cancellationToken);

            return result.ToList();
        }
    }
}
