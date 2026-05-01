using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Repositories;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class DocumentTypeRepository : EntityRepository<DocumentType>, IDocumentTypeRepository
{
    public DocumentTypeRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }
}
