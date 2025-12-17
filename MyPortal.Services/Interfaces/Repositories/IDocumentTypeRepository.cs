using MyPortal.Contracts.Models;
using MyPortal.Core.Entities;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces.Repositories.Base;

namespace MyPortal.Services.Interfaces.Repositories;

public interface IDocumentTypeRepository : IEntityRepository<DocumentType>
{
    Task<IList<LookupResponse>> GetDocumentTypes(DocumentTypeFilter filter, CancellationToken cancellationToken);
}