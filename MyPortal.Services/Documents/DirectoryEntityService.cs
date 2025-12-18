using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.Documents;

public abstract class DirectoryEntityService<TDirectoryEntity> : BaseService, IDirectoryEntityService<TDirectoryEntity> where TDirectoryEntity : IDirectoryEntity
{
    protected readonly IDirectoryService DirectoryService;
    private readonly IDocumentService _documentService;
    private readonly IValidationService _validationService;

    protected DirectoryEntityService(IAuthorizationService authorizationService, IDirectoryService directoryService,
        IDocumentService documentService, IValidationService validationService) : base(authorizationService)
    {
        DirectoryService = directoryService;
        _documentService = documentService;
        _validationService = validationService;
    }

    public abstract Task<TDirectoryEntity?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken);

    public virtual async Task<bool> CanViewDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        return await EntityRootContainsDirectory(entityId, directoryId, cancellationToken);
    }

    public virtual async Task<bool> CanEditDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        return await EntityRootContainsDirectory(entityId, directoryId, cancellationToken);
    }

    public async Task<DirectoryDetailsResponse> CreateDirectoryAsync(Guid entityId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _validationService.ValidateAsync(model);

        if (!await CanEditDocumentsAsync(entityId, model.ParentId!.Value, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to create directories here.");
        }

        return await DirectoryService.CreateDirectoryAsync(model, cancellationToken);
    }

    public async Task<DirectoryDetailsResponse> UpdateDirectoryAsync(Guid entityId, Guid directoryId,
        DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _validationService.ValidateAsync(model);

        if (!await CanEditDocumentsAsync(entityId, directoryId, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to edit this directory.");
        }

        return await DirectoryService.UpdateDirectoryAsync(directoryId, model, cancellationToken);
    }

    public async Task DeleteDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken)
    {
        if (await CanEditDocumentsAsync(entityId, directoryId, cancellationToken))
        {
            var entity = await GetByIdAsync(entityId, cancellationToken);

            if (entity == null)
            {
                throw new NotFoundException("Directory owner not found.");
            }

            if (entity.DirectoryId == directoryId)
            {
                throw new ForbiddenException("You cannot delete the root directory of this entity.");
            }

            await DirectoryService.DeleteDirectoryAsync(directoryId, cancellationToken);
        }
        else
        {
            throw new ForbiddenException("You do not have permission to delete this directory.");
        }
    }

    public async Task<DirectoryDetailsResponse?> GetDirectoryByIdAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        if (await CanViewDocumentsAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryByIdAsync(directoryId, cancellationToken);
        }

        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DirectoryContentsResponse> GetDirectoryContentsAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken)
    {
        if (await CanViewDocumentsAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryContentsAsync(directoryId, cancellationToken);
        }
        
        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken,
        bool includeDeletedDocs = true)
    {
        if (await CanViewDocumentsAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryTreeAsync(directoryId, cancellationToken, includeDeletedDocs);
        }
        
        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DocumentDetailsResponse> CreateDocumentAsync(Guid entityId, DocumentUpsertRequest model, CancellationToken cancellationToken)
    {
        if (await CanEditDocumentsAsync(entityId, model.DirectoryId, cancellationToken))
        {
            return await _documentService.CreateDocumentAsync(model, cancellationToken);
        }
        
        throw new ForbiddenException("You do not have permission to create documents here.");
    }

    public async Task<DocumentDetailsResponse> UpdateDocumentAsync(Guid entityId, Guid documentId, DocumentUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Directory owner not found.");
        }

        if (await CanEditDocumentsAsync(entityId, entity.DirectoryId, cancellationToken))
        {
            return await _documentService.UpdateDocumentAsync(documentId, model, cancellationToken);
        }
        
        throw new ForbiddenException("You do not have permission to edit this document.");
    }

    public async Task DeleteDocumentAsync(Guid entityId, Guid documentId, CancellationToken cancellationToken,
        bool softDelete = true)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Directory owner not found.");
        }

        if (await CanEditDocumentsAsync(entityId, entity.DirectoryId, cancellationToken))
        {
            await _documentService.DeleteDocumentAsync(documentId, cancellationToken);
        }
        else
        {
            throw new ForbiddenException("You do not have permission to delete this document.");
        }
    }

    public async Task<DocumentDetailsResponse?> GetDocumentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Directory owner not found.");
        }

        if (await CanViewDocumentsAsync(entityId, entity.DirectoryId, cancellationToken))
        {
            return await _documentService.GetDocumentByIdAsync(documentId, cancellationToken);
        }

        throw new ForbiddenException("You do not have permission to view this document.");
    }

    public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Directory owner not found.");
        }

        if (await CanViewDocumentsAsync(entityId, entity.DirectoryId, cancellationToken))
        {
            return await _documentService.GetDocumentWithContentByIdAsync(documentId, cancellationToken);
        }

        throw new ForbiddenException("You do not have permission to view this document.");
    }

    private async Task<bool> EntityRootContainsDirectory(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException("Directory owner not found.");
        }
        
        var rootTree = await DirectoryService.GetFlatDirectoryTreeAsync(entity.DirectoryId, cancellationToken);

        return rootTree.Directories.Any(d => d.Id == directoryId);
    }
}