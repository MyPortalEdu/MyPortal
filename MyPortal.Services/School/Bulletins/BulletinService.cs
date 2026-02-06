using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.School.Bulletins;

public class BulletinService : DirectoryEntityService<Bulletin>, IBulletinService
{
    private readonly IBulletinRepository _bulletinRepository;

    public BulletinService(IAuthorizationService authorizationService, IDirectoryService directoryService,
        IDocumentService documentService, IValidationService validationService,
        IBulletinRepository bulletinRepository) : base(authorizationService, directoryService, documentService,
        validationService)
    {
        _bulletinRepository = bulletinRepository;
    }

    public async Task<BulletinDetailsResponse?> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);
        
        var bulletin = await _bulletinRepository.GetDetailsByIdAsync(bulletinId, scope, cancellationToken);

        return bulletin ?? throw new NotFoundException("Bulletin not found.");
    }

    public async Task<PageResult<BulletinSummaryResponse>> GetBulletinsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var scope = await  BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);
        
        return await _bulletinRepository.GetSummariesAsync(scope, filter, sort, paging, cancellationToken);
    }

    public async Task<Guid> CreateBulletinAsync(BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var bulletinId = SqlConvention.SequentialGuid();

        var directoryRequest = new DirectoryUpsertRequest
        {
            Name = $"bulletin-{bulletinId:N}"
        };

        var directory = await DirectoryService.CreateDirectoryAsync(directoryRequest, cancellationToken);

        var bulletin = new Bulletin
        {
            Id = SqlConvention.SequentialGuid(),
            Title = model.Title,
            Detail = model.Detail,
            IsPrivate = model.IsPrivate,
            ExpiresAt = model.ExpiresAt,
            DirectoryId = directory.Id
        };

        var result = await _bulletinRepository.InsertAsync(bulletin, cancellationToken);

        return result.Id;
    }

    public async Task UpdateBulletinAsync(Guid bulletinId, BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }
        
        var scope = await BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);

        if (!BulletinAccessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to edit this bulletin.");
        }

        bulletin.Title = model.Title;
        bulletin.Detail = model.Detail;
        bulletin.IsPrivate = model.IsPrivate;
        bulletin.ExpiresAt = model.ExpiresAt;

        if (!await _authorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins,
                cancellationToken))
        {
            // Any edits by non-approvers will require re-approval
            bulletin.IsApproved = false;
        }

        await _bulletinRepository.UpdateAsync(bulletin, cancellationToken);
    }

    public async Task DeleteBulletinAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }
        
        var scope = await BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);

        if (!BulletinAccessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to delete this bulletin.");
        }

        await _bulletinRepository.DeleteAsync(bulletinId, cancellationToken);

        await DirectoryService.DeleteDirectoryAsync(bulletin.DirectoryId, cancellationToken);
    }

    public async Task UpdateBulletinApprovalAsync(Guid bulletinId, bool isApproved, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.ApproveSchoolBulletins,
            cancellationToken);

        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        bulletin.IsApproved = isApproved;

        await _bulletinRepository.UpdateAsync(bulletin, cancellationToken);
    }

    public override async Task<Bulletin?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(entityId, cancellationToken);

        return bulletin;
    }

    

    public override async Task<bool> CanViewDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var scope =  await BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);
        
        var bulletin = await GetByIdAsync(entityId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }
        
        if (BulletinAccessPolicy.CanView(bulletin, scope, DateTime.UtcNow))
        {
            return await base.CanViewDocumentsAsync(entityId, directoryId, cancellationToken);
        }

        return false;
    }

    public override async Task<bool> CanEditDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(_authorizationService, cancellationToken);
        
        var bulletin = await GetByIdAsync(entityId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }
        
        if (BulletinAccessPolicy.CanEdit(bulletin, scope))
        {
            return await base.CanEditDocumentsAsync(entityId, directoryId, cancellationToken);
        }

        return false;
    }
}