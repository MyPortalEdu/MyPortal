using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Services.Filters;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Interfaces.Documents
{
    public interface IDocumentService
    {
        Task<DocumentDetailsResponse> CreateAsync(DocumentUpsertRequest model, CancellationToken cancellationToken);
        Task<DocumentDetailsResponse> UpdateAsync(Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken);
        Task DeleteAsync(Guid documentId, CancellationToken cancellationToken, bool softDelete = true);
        Task<DocumentDetailsResponse> GetDocumentByIdAsync(Guid documentId, CancellationToken cancellationToken);
        Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid documentId,
            CancellationToken cancellationToken);
        Task<IList<LookupResponse>> GetDocumentTypesAsync(DocumentTypeFilter filter, CancellationToken cancellationToken);
    }
}
