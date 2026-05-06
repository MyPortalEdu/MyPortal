using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Services.Documents;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces.Security;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.School;

namespace MyPortal.Services.School.Bulletins;

public class BulletinService : DirectoryEntityService<Bulletin>, IBulletinService
{
    private readonly IBulletinRepository _bulletinRepository;
    private readonly IAccessPolicy<Bulletin, BulletinVisibilityScope> _accessPolicy;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public BulletinService(IAuthorizationService authorizationService, ILogger<BulletinService> logger,
        IDirectoryService directoryService, IDocumentService documentService, IValidationService validationService,
        IBulletinRepository bulletinRepository, IAccessPolicy<Bulletin, BulletinVisibilityScope> accessPolicy,
        IUnitOfWorkFactory unitOfWorkFactory) : base(
        authorizationService, logger, directoryService, documentService, validationService)
    {
        _bulletinRepository = bulletinRepository;
        _accessPolicy = accessPolicy;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<BulletinDetailsResponse> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        // Defense in depth: assert the access policy server-side before issuing the
        // detail-projection query. The repository SP also filters by scope, but we don't
        // want IDOR protection to depend on a single layer.
        _ = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        var bulletin = await _bulletinRepository.GetDetailsByIdAsync(bulletinId, scope, cancellationToken);

        return bulletin ?? throw new NotFoundException("Bulletin not found.");
    }

    public async Task<PageResult<BulletinSummaryResponse>> GetBulletinSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var scope = await  BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);
        
        return await _bulletinRepository.GetSummariesAsync(scope, filter, sort, paging, cancellationToken);
    }

    public async Task<Guid> CreateBulletinAsync(BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var bulletinId = SqlConvention.SequentialGuid();

        var directoryRequest = new DirectoryUpsertRequest
        {
            Name = $"bulletin-{bulletinId:N}",
            IsPrivate = model.IsPrivate,
            UploadPolicy = DirectoryUploadPolicy.StaffOnly
        };

        await _unitOfWorkFactory.RunInTransactionAsync(uow: null, async uow =>
        {
            var directory = await DirectoryService.CreateDirectoryAsync(directoryRequest, cancellationToken, uow);

            Logger.LogInformation("Directory created for bulletin: {bulletinId}", bulletinId);

            var bulletin = new Bulletin
            {
                Id = bulletinId,
                Title = model.Title,
                Detail = model.Detail,
                IsPrivate = model.IsPrivate,
                ExpiresAt = model.ExpiresAt,
                DirectoryId = directory.Id
            };

            await _bulletinRepository.InsertAsync(bulletin, cancellationToken, uow.Transaction);
        }, cancellationToken);

        Logger.LogInformation("Bulletin created: {bulletinId}", bulletinId);

        return bulletinId;
    }

    public async Task UpdateBulletinAsync(Guid bulletinId, BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to edit this bulletin.");
        }

        bulletin.Title = model.Title;
        bulletin.Detail = model.Detail;
        bulletin.IsPrivate = model.IsPrivate;
        bulletin.ExpiresAt = model.ExpiresAt;
        // Hand the client's expected version to the repo's optimistic-concurrency check;
        // QueryKit's UpdateWithVersionAsync turns it into a WHERE Version=@expected guard
        // and throws ConcurrencyException on mismatch.
        bulletin.Version = model.ExpectedVersion;

        if (!await AuthorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins,
                cancellationToken))
        {
            // Any edits by non-approvers will require re-approval
            bulletin.IsApproved = false;
        }

        await _bulletinRepository.UpdateAsync(bulletin, cancellationToken);
        Logger.LogInformation("Bulletin updated: {bulletinId}", bulletinId);
    }

    public async Task DeleteBulletinAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to delete this bulletin.");
        }

        await _unitOfWorkFactory.RunInTransactionAsync(uow: null, async uow =>
        {
            await _bulletinRepository.DeleteAsync(bulletinId, cancellationToken, transaction: uow.Transaction);

            await DirectoryService.DeleteDirectoryAsync(bulletin.DirectoryId, cancellationToken, uow);
        }, cancellationToken);

        Logger.LogInformation("Bulletin deleted: {bulletinId}", bulletinId);
        Logger.LogInformation("Directory deleted for bulletin: {bulletinId}", bulletinId);
    }

    public async Task UpdateBulletinApprovalAsync(Guid bulletinId, bool isApproved, long expectedVersion,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ApproveSchoolBulletins,
            cancellationToken);

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to approve this bulletin.");
        }

        bulletin.IsApproved = isApproved;
        // Hand the client's expected version to the repo's optimistic-concurrency check.
        bulletin.Version = expectedVersion;

        await _bulletinRepository.UpdateAsync(bulletin, cancellationToken);

        Logger.LogInformation("Bulletin approval updated: {bulletinId}, IsApproved: {isApproved}", bulletinId,
            isApproved);
    }

    public override async Task<Bulletin> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(entityId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        return bulletin;
    }

    public override async Task<bool> CanViewDirectoryAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetBulletinOrThrowAsync(entityId, cancellationToken);

        if (!_accessPolicy.CanView(bulletin, scope))
        {
            return false;
        }

        return await CanStructurallyViewDirectoryAsync(entityId, directoryId, cancellationToken);
    }

    public override async Task<bool> CanEditDirectoryAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetBulletinOrThrowAsync(entityId, cancellationToken);

        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            return false;
        }

        return await CanStructurallyEditDirectoryAsync(entityId, directoryId, cancellationToken);
    }

    public override async Task<bool> CanUploadToDirectoryAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetBulletinOrThrowAsync(entityId, cancellationToken);

        // Upload to a bulletin's directory is gated by CanEdit on the bulletin — only the
        // creator (with edit permission) or an approver can attach documents to it.
        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            return false;
        }

        return await CanStructurallyUploadToDirectoryAsync(entityId, directoryId, cancellationToken);
    }

    private async Task<Bulletin> GetBulletinOrThrowAsync(Guid bulletinId, CancellationToken ct)
    {
        return await _bulletinRepository.GetByIdAsync(bulletinId, ct)
               ?? throw new NotFoundException("Bulletin not found.");
    }

    private async Task<Bulletin> GetVisibleBulletinOrThrowAsync(Guid bulletinId, BulletinVisibilityScope scope,
        CancellationToken ct)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, ct);

        if (bulletin == null || !_accessPolicy.CanView(bulletin, scope))
        {
            throw new NotFoundException("Bulletin not found.");
        }

        return bulletin;
    }
}