using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Interfaces.Services
{
    public interface IDocumentService
    {
        Task<DocumentDetailsResponse> CreateDocumentAsync(DocumentUpsertRequest model, CancellationToken cancellationToken);

        Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken);

        Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken);

        Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken);

        Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId, CancellationToken cancellationToken);
    }
}
