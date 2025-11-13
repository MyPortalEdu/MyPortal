using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories
{
    public interface IDocumentRepository : IEntityRepository<Document>
    {
        Task<DocumentDetailsResponse?> GetDetailsByIdAsync(Guid documentId, CancellationToken cancellationToken);

        Task<IList<DocumentDetailsResponse>> GetDocumentsByDirectoryId(Guid directoryId,
            CancellationToken cancellationToken);
    }
}
