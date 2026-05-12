using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Documents;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.School;
using MyPortal.Services.Interfaces.Security;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.School.Bulletins;

public class BulletinService : DirectoryEntityService<Bulletin>, IBulletinService
{
    private readonly IBulletinRepository _bulletinRepository;
    private readonly IBulletinAcknowledgementRepository _ackRepository;
    private readonly IAccessPolicy<Bulletin, BulletinVisibilityScope> _accessPolicy;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public BulletinService(IAuthorizationService authorizationService, ILogger<BulletinService> logger,
        IDirectoryService directoryService, IDocumentService documentService, IValidationService validationService,
        IBulletinRepository bulletinRepository, IBulletinAcknowledgementRepository ackRepository,
        IAccessPolicy<Bulletin, BulletinVisibilityScope> accessPolicy,
        IUnitOfWorkFactory unitOfWorkFactory) : base(
        authorizationService, logger, directoryService, documentService, validationService)
    {
        _bulletinRepository = bulletinRepository;
        _ackRepository = ackRepository;
        _accessPolicy = accessPolicy;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<BulletinDetailsResponse> GetDetailsByIdAsync(Guid bulletinId,
        CancellationToken cancellationToken)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        // The SP enforces audience-membership filtering for non-staff and returns no
        // header row when the bulletin isn't visible to the caller — that's how we
        // map non-visible to 404, same as a genuinely-missing id.
        var bulletin = await _bulletinRepository.GetDetailsByIdAsync(bulletinId, scope, cancellationToken);

        return bulletin ?? throw new NotFoundException("Bulletin not found.");
    }

    public async Task<PageResult<BulletinSummaryResponse>> GetBulletinSummariesAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        return await _bulletinRepository.GetSummariesAsync(scope, filter, sort, paging, cancellationToken);
    }

    public async Task<Guid> CreateAsync(BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        if (model.IsPinned)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.School.PinSchoolBulletins, cancellationToken);
        }

        ValidateAudiences(model.Audiences);

        var bulletinId = SqlConvention.SequentialGuid();

        // The bulletin's directory is staff-upload-only and not externally browsable:
        // attachments are accessed via the bulletin's CanView path. Audience targeting
        // (not a directory flag) determines who reads the bulletin itself.
        var directoryRequest = new DirectoryUpsertRequest
        {
            Name = $"bulletin-{bulletinId:N}",
            IsPrivate = true,
            UploadPolicy = DirectoryUploadPolicy.StaffOnly
        };

        await _unitOfWorkFactory.RunInTransactionAsync(uow: null, async uow =>
        {
            var directory = await DirectoryService.CreateAsync(directoryRequest, cancellationToken, uow);

            Logger.LogInformation("Directory created for bulletin: {bulletinId}", bulletinId);

            var bulletin = new Bulletin
            {
                Id = bulletinId,
                Title = model.Title,
                Detail = model.Detail,
                CategoryId = model.CategoryId,
                RequiresAcknowledgement = model.RequiresAcknowledgement,
                ExpiresAt = model.ExpiresAt,
                PinnedAt = model.IsPinned ? DateTime.UtcNow : null,
                DirectoryId = directory.Id
            };

            await _bulletinRepository.InsertAsync(bulletin, cancellationToken, uow.Transaction);

            var audienceRows = ToAudienceEntities(bulletinId, model.Audiences);
            await _bulletinRepository.ReplaceAudiencesAsync(bulletinId, audienceRows, cancellationToken,
                uow.Transaction);
        }, cancellationToken);

        Logger.LogInformation("Bulletin created: {bulletinId}", bulletinId);
        return bulletinId;
    }

    public async Task UpdateAsync(Guid bulletinId, BulletinUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        ValidateAudiences(model.Audiences);

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            throw new ForbiddenException("You do not have permission to edit this bulletin.");
        }

        // Pin state may only be changed by someone with the pin permission. Toggling
        // is the trigger: pinning, unpinning, or no-change-but-already-pinned all
        // require the permission only when the value actually changes.
        var wasPinned = bulletin.PinnedAt.HasValue;
        if (wasPinned != model.IsPinned)
        {
            await AuthorizationService.RequirePermissionAsync(Permissions.School.PinSchoolBulletins,
                cancellationToken);
            bulletin.PinnedAt = model.IsPinned ? DateTime.UtcNow : null;
        }

        bulletin.Title = model.Title;
        bulletin.Detail = model.Detail;
        bulletin.CategoryId = model.CategoryId;
        bulletin.RequiresAcknowledgement = model.RequiresAcknowledgement;
        bulletin.ExpiresAt = model.ExpiresAt;
        // Hand the client's expected version to the repo's optimistic-concurrency check;
        // QueryKit's UpdateWithVersionAsync turns it into a WHERE Version=@expected guard
        // and throws ConcurrencyException on mismatch.
        bulletin.Version = model.ExpectedVersion;

        await _unitOfWorkFactory.RunInTransactionAsync(uow: null, async uow =>
        {
            await _bulletinRepository.UpdateAsync(bulletin, cancellationToken, uow.Transaction);

            var audienceRows = ToAudienceEntities(bulletinId, model.Audiences);
            await _bulletinRepository.ReplaceAudiencesAsync(bulletinId, audienceRows, cancellationToken,
                uow.Transaction);
        }, cancellationToken);

        Logger.LogInformation("Bulletin updated: {bulletinId}", bulletinId);
    }

    public async Task DeleteAsync(Guid bulletinId, CancellationToken cancellationToken)
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
            // Order matters: the bulletin holds an FK to its directory
            // (FK_Bulletins_DirectoryId_Directories), so deleting the directory
            // first throws "DELETE statement conflicted with the REFERENCE
            // constraint" as soon as the directory actually has content (e.g.
            // an attachment makes the path hit the live FK). Drop the bulletin
            // first to release that reference, then hard-delete the directory.
            //
            // BulletinAudiences and BulletinAcknowledgements cascade on the FK,
            // so a single Bulletins delete reaps the lot. The directory is
            // owned by the bulletin and is cleaned up alongside, including any
            // attachments inside it (DirectoryService.DeleteAsync recurses).
            await _bulletinRepository.DeleteAsync(bulletinId, cancellationToken, transaction: uow.Transaction);

            await DirectoryService.DeleteAsync(bulletin.DirectoryId, cancellationToken, uow, false);
        }, cancellationToken);

        Logger.LogInformation("Bulletin deleted: {bulletinId}", bulletinId);
    }

    public async Task UpdatePinAsync(Guid bulletinId, bool isPinned, long expectedVersion,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.PinSchoolBulletins, cancellationToken);

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        var bulletin = await GetVisibleBulletinOrThrowAsync(bulletinId, scope, cancellationToken);

        // Pin permission alone is enough — pinners are admins and can pin any visible
        // bulletin, regardless of authorship.
        bulletin.PinnedAt = isPinned ? DateTime.UtcNow : null;
        bulletin.Version = expectedVersion;

        await _bulletinRepository.UpdateAsync(bulletin, cancellationToken);

        Logger.LogInformation("Bulletin pin updated: {bulletinId}, IsPinned: {isPinned}", bulletinId, isPinned);
    }

    public async Task AcknowledgeAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var userId = AuthorizationService.GetCurrentUserId()
                     ?? throw new AuthenticationException("Not authenticated.");

        var scope = await BulletinVisibilityScope.FromAsync(AuthorizationService, cancellationToken);

        // GetDetailsByIdAsync uses the audience-aware SP; if the caller isn't in the
        // audience the result is null and we 404 — same shape as for missing ids.
        var details = await _bulletinRepository.GetDetailsByIdAsync(bulletinId, scope, cancellationToken);
        if (details is null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        if (!details.RequiresAcknowledgement)
        {
            throw new InvalidOperationException("This bulletin does not require acknowledgement.");
        }

        var inserted = await _ackRepository.AcknowledgeAsync(bulletinId, userId, cancellationToken);

        if (inserted)
        {
            Logger.LogInformation("Bulletin acknowledged: {bulletinId} by {userId}", bulletinId, userId);
        }
    }

    public override async Task<Bulletin> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        return await _bulletinRepository.GetByIdAsync(entityId, cancellationToken)
               ?? throw new NotFoundException("Bulletin not found.");
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
        // creator (with edit permission) or a pinner can attach documents to it.
        if (!_accessPolicy.CanEdit(bulletin, scope))
        {
            return false;
        }

        return await CanStructurallyUploadToDirectoryAsync(entityId, directoryId, cancellationToken);
    }

    private static void ValidateAudiences(IList<BulletinAudienceRequest> audiences)
    {
        // The validator catches the empty / malformed cases. We re-check here so the
        // service is defensible when called outside the controller pipeline (background
        // jobs, integration tests bypassing FluentValidation, etc.).
        if (audiences is null || audiences.Count == 0)
        {
            throw new InvalidOperationException("A bulletin must target at least one audience.");
        }

        foreach (var a in audiences)
        {
            var needsGroup = a.AudienceKind == BulletinAudienceKind.StudentGroup;
            if (needsGroup && a.StudentGroupId is null)
            {
                throw new InvalidOperationException("StudentGroup audience entries require a StudentGroupId.");
            }
            if (!needsGroup && a.StudentGroupId is not null)
            {
                throw new InvalidOperationException(
                    "StudentGroupId must only be set when AudienceKind is StudentGroup.");
            }
        }
    }

    private static IList<BulletinAudience> ToAudienceEntities(Guid bulletinId,
        IList<BulletinAudienceRequest> audiences) =>
        audiences.Select(a => new BulletinAudience
        {
            Id = Guid.NewGuid(),
            BulletinId = bulletinId,
            AudienceKind = a.AudienceKind,
            StudentGroupId = a.StudentGroupId
        }).ToList();

    private async Task<Bulletin> GetBulletinOrThrowAsync(Guid bulletinId, CancellationToken ct) =>
        await _bulletinRepository.GetByIdAsync(bulletinId, ct)
        ?? throw new NotFoundException("Bulletin not found.");

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
