using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.Documents;

public abstract class DirectoryEntityService<TDirectoryEntity> : BaseService, IDirectoryEntityService<TDirectoryEntity> where TDirectoryEntity : IDirectoryEntity
{
    private readonly IDocumentService _documentService;
    private readonly IValidationService _validationService;

    protected IDirectoryService DirectoryService { get; }

    protected DirectoryEntityService(IAuthorizationService authorizationService, 
        ILogger<DirectoryEntityService<TDirectoryEntity>> logger,
        IDirectoryService directoryService, IDocumentService documentService,
        IValidationService validationService) : base(authorizationService, logger)
    {
        DirectoryService = directoryService;
        _documentService = documentService;
        _validationService = validationService;
    }

    public abstract Task<TDirectoryEntity> GetByIdAsync(Guid entityId, CancellationToken cancellationToken);

    public virtual async Task<bool> CanViewDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        var dir = await TryGetDirectoryScopedAsync(entityId, directoryId, ct);
        
        if (dir == null)
        {
            return false;
        }

        return !(dir.IsPrivate && AuthorizationService.GetCurrentUserType() != UserType.Staff);
    }

    public virtual async Task<bool> CanEditDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        if (AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            return false;
        }

        return await TryGetDirectoryScopedAsync(entityId, directoryId, ct) != null;
    }

    public virtual async Task<bool> CanUploadToDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        var dir = await TryGetDirectoryScopedAsync(entityId, directoryId, ct);
        
        if (dir == null)
        {
            return false;
        }

        var userType = AuthorizationService.GetCurrentUserType();

        if (dir.IsPrivate && userType != UserType.Staff)
        {
            return false;
        }

        return userType switch
        {
            UserType.Staff => true,
            UserType.Student => dir.UploadPolicy is DirectoryUploadPolicy.StaffAndStudents
                or DirectoryUploadPolicy.StaffStudentsParents,
            UserType.Parent => dir.UploadPolicy is DirectoryUploadPolicy.StaffAndParents
                or DirectoryUploadPolicy.StaffStudentsParents,
            _ => false
        };
    }

    public virtual async Task<bool> CanViewDocumentAsync(Guid entityId, DocumentDetailsResponse document,
        CancellationToken cancellationToken)
    {
        var canViewDir = await CanViewDirectoryAsync(entityId, document.DirectoryId, cancellationToken);

        if (!canViewDir)
        {
            return false;
        }

        if (document.IsPrivate)
        {
            if (AuthorizationService.GetCurrentUserType() != UserType.Staff)
            {
                return false;
            }
        }

        return true;
    }

    public virtual async Task<bool> CanEditDocumentAsync(Guid entityId, DocumentDetailsResponse document,
        CancellationToken cancellationToken)
    {
        var canViewDir = await CanViewDirectoryAsync(entityId, document.DirectoryId, cancellationToken);

        if (!canViewDir)
        {
            return false;
        }

        if (AuthorizationService.GetCurrentUserType() != UserType.Staff)
        {
            if (document.IsPrivate)
            {
                return false;
            }

            if (document.CreatedById != AuthorizationService.GetCurrentUserId())
            {
                return false;
            }
        }

        return true;
    }

    public async Task<DirectoryDetailsResponse> CreateDirectoryAsync(Guid entityId, DirectoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _validationService.ValidateAsync(model);

        if (!await CanEditDirectoryAsync(entityId, model.ParentId!.Value, cancellationToken))
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

        if (!await CanEditDirectoryAsync(entityId, directoryId, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to edit this directory.");
        }

        return await DirectoryService.UpdateDirectoryAsync(directoryId, model, cancellationToken);
    }

    public async Task DeleteDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken)
    {
        if (await CanEditDirectoryAsync(entityId, directoryId, cancellationToken))
        {
            var entity = await GetByIdAsync(entityId, cancellationToken);

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

    public async Task<DirectoryDetailsResponse> GetDirectoryByIdAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        if (await CanViewDirectoryAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryByIdAsync(directoryId, cancellationToken);
        }

        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DirectoryContentsResponse> GetDirectoryContentsAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken)
    {
        if (await CanViewDirectoryAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryContentsAsync(directoryId, cancellationToken);
        }
        
        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DirectoryTreeResponse> GetDirectoryTreeAsync(Guid entityId, Guid directoryId, CancellationToken cancellationToken,
        bool includeDeletedDocs = true)
    {
        if (await CanViewDirectoryAsync(entityId, directoryId, cancellationToken))
        {
            return await DirectoryService.GetDirectoryTreeAsync(directoryId, cancellationToken, includeDeletedDocs);
        }
        
        throw new ForbiddenException("You do not have permission to view this directory.");
    }

    public async Task<DocumentDetailsResponse> CreateDocumentAsync(Guid entityId, DocumentUpsertRequest model, CancellationToken cancellationToken)
    {
        if (await CanUploadToDirectoryAsync(entityId, model.DirectoryId, cancellationToken))
        {
            return await _documentService.CreateDocumentAsync(model, cancellationToken);
        }
        
        throw new ForbiddenException("You do not have permission to create documents here.");
    }

    public async Task<DocumentDetailsResponse> UpdateDocumentAsync(
        Guid entityId, Guid documentId, DocumentUpsertRequest model, CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentByIdAsync(documentId, cancellationToken);

        if (!await CanEditDocumentAsync(entityId, document, cancellationToken))
            throw new ForbiddenException("You do not have permission to edit this document.");

        // Only require destination permission if moving directories
        if (model.DirectoryId != document.DirectoryId &&
            !await CanUploadToDirectoryAsync(entityId, model.DirectoryId, cancellationToken))
            throw new ForbiddenException("You do not have permission to edit the destination directory.");

        return await _documentService.UpdateDocumentAsync(documentId, model, cancellationToken);
    }

    public async Task DeleteDocumentAsync(Guid entityId, Guid documentId, CancellationToken cancellationToken,
        bool softDelete = true)
    {
        var document = await _documentService.GetDocumentByIdAsync(documentId, cancellationToken);

        if (await CanEditDocumentAsync(entityId, document, cancellationToken))
        {
            await _documentService.DeleteDocumentAsync(documentId, cancellationToken, softDelete);
        }
        else
        {
            throw new ForbiddenException("You do not have permission to delete this document.");
        }
    }

    public async Task<DocumentDetailsResponse> GetDocumentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentByIdAsync(documentId, cancellationToken);

        if (await CanViewDocumentAsync(entityId, document, cancellationToken))
        {
            return document;
        }

        throw new ForbiddenException("You do not have permission to view this document.");
    }

    public async Task<DocumentContentResponse> GetDocumentWithContentByIdAsync(Guid entityId, Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocumentByIdAsync(documentId, cancellationToken);

        if (await CanViewDocumentAsync(entityId, document, cancellationToken))
        {
            return await _documentService.GetDocumentWithContentByIdAsync(documentId, cancellationToken);
        }

        throw new ForbiddenException("You do not have permission to view this document.");
    }

    private async Task<DirectoryDetailsResponse?> TryGetDirectoryScopedAsync(
        Guid entityId,
        Guid directoryId,
        CancellationToken ct)
    {
        // cheap “does it exist” first (optional, depends on your SQL)
        var dir = await DirectoryService.TryGetDirectoryByIdAsync(directoryId, ct);
        
        if (dir == null)
        {
            return null;
        }

        // prevent cross-entity access even if caller guesses a valid Guid
        var inSubtree = await DirectoryService.IsDirectoryInSubtreeAsync(
            rootDirectoryId: (await GetByIdAsync(entityId, ct)).DirectoryId,
            candidateDirectoryId: directoryId,
            ct);

        if (!inSubtree)
        {
            return null;
        }

        return dir;
    }
}