using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Core.Entities;
using MyPortal.Data.Repositories.Base;
using MyPortal.Services.Extensions;
using MyPortal.Services.Filters;
using MyPortal.Services.Interfaces.Repositories;

namespace MyPortal.Data.Repositories;

public class DocumentTypeRepository : EntityRepository<DocumentType>, IDocumentTypeRepository
{
    public DocumentTypeRepository(IDbConnectionFactory factory) : base(factory)
    {
    }

    public async Task <IList<LookupResponse>> GetDocumentTypes(DocumentTypeFilter filter, CancellationToken cancellationToken)
    {
        var result = await GetListAsync(cancellationToken: cancellationToken);

        var query = result.Where(t => t.Active);

        query = query.Where(t =>
            (filter.General && t.General) || (filter.Student && t.Student) || (filter.Contact && t.Contact) ||
            (filter.Send && t.IsSend) || (filter.Staff && t.Staff));

        return query.Select(t => t.ToResponseModel()).ToList();
    }
}