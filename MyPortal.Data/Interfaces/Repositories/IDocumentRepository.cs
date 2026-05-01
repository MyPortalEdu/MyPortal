using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Repositories.Base;

namespace MyPortal.Data.Interfaces.Repositories
{
    public interface IDocumentRepository : IEntityRepository<Document>
    {
        Task<DocumentDetailsResponse> GetDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken);

        Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsByDirectoryId(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false);

        Task<IReadOnlyList<DocumentDetailsResponse>> GetDocumentsInSubtreeAsync(Guid directoryId,
            CancellationToken cancellationToken, bool includeDeleted = false);
    }
}
