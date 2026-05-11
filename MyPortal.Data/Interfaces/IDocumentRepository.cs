using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces
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
