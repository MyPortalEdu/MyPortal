using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Services.Documents;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Repositories.Enums;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.School;

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

    public async Task<BulletinDetailsDto?> GetDetailsByIdAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetDetailsByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        if (!bulletin.IsApproved)
        {
            await _authorizationService.RequirePermissionAsync(Permissions.School.ApproveSchoolBulletins,
                cancellationToken);
        }

        if (bulletin.IsPrivate)
        {
            _authorizationService.RequireUserType(UserType.Staff);
        }

        return bulletin;
    }

    public async Task<PageResult<BulletinSummaryDto>> GetBulletinsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        if (!await _authorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins,
                cancellationToken))
        {
            filter = ExcludeNonApprovedBulletins(filter);
        }

        if (_authorizationService.GetCurrentUserType() != UserType.Staff)
        {
            filter = ExcludePrivateBulletins(filter);
        }

        return await _bulletinRepository.GetBulletinsAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<Guid> CreateBulletinAsync(BulletinUpsertDto model, CancellationToken cancellationToken)
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

    public async Task UpdateBulletinAsync(Guid bulletinId, BulletinUpsertDto model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
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
        await _authorizationService.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);

        var bulletin = await _bulletinRepository.GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
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

    private FilterOptions ExcludeNonApprovedBulletins(FilterOptions? filter)
    {
        var excludeNonApproved = new FilterCriterion
        {
            ColumnName = "IsApproved",
            Operator = FilterOperator.Equals,
            Value = true
        };

        var includeOwn = new FilterCriterion
        {
            ColumnName = "CreatedById",
            Operator = FilterOperator.Equals,
            Value = _authorizationService.GetCurrentUserId()
        };

        ApplyFilterCriteria(filter, BoolJoin.Or, excludeNonApproved, includeOwn);

        return filter!;
    }

    private FilterOptions ExcludePrivateBulletins(FilterOptions? filter)
    {
        var excludePrivate = new FilterCriterion()
        {
            ColumnName = "IsPrivate",
            Operator = FilterOperator.Equals,
            Value = false
        };

        ApplyFilterCriteria(filter, BoolJoin.And, excludePrivate);

        return filter!;

        var filterOptions = new FilterOptions
        {
            Groups = new[]
            {
                new FilterGroup
                {
                    Criteria = new[]
                    {
                        new FilterCriterion
                            { ColumnName = "IsPrivate", Value = false, Operator = FilterOperator.Equals }
                    }
                }
            }
        };
    }

    public override async Task<Bulletin?> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var bulletin = await _bulletinRepository.GetByIdAsync(entityId, cancellationToken);

        return bulletin;
    }

    private async Task<bool> CanViewBulletinAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        var bulletin = await GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        if (!bulletin.IsApproved)
        {
            if (await _authorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins,
                    cancellationToken))
            {
                return true;
            }

            return bulletin.CreatedById == _authorizationService.GetCurrentUserId();
        }

        if (bulletin.IsPrivate)
        {
            return _authorizationService.GetCurrentUserType() == UserType.Staff;
        }

        return bulletin.IsApproved;
    }

    private async Task<bool> CanEditBulletinAsync(Guid bulletinId, CancellationToken cancellationToken)
    {
        if (await _authorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins,
                cancellationToken))
        {
            return true;
        }

        var bulletin = await GetByIdAsync(bulletinId, cancellationToken);

        if (bulletin == null)
        {
            throw new NotFoundException("Bulletin not found.");
        }

        // Users can edit their own bulletins if they have EditSchoolBulletins permission
        return bulletin.CreatedById == _authorizationService.GetCurrentUserId() &&
               await _authorizationService.HasPermissionAsync(Permissions.School.EditSchoolBulletins,
                   cancellationToken);
    }

    public override async Task<bool> CanViewDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        if (await CanViewBulletinAsync(entityId, cancellationToken))
        {
            return await base.CanViewDocumentsAsync(entityId, directoryId, cancellationToken);
        }

        return false;
    }

    public override async Task<bool> CanEditDocumentsAsync(Guid entityId, Guid directoryId,
        CancellationToken cancellationToken)
    {
        if (await CanEditBulletinAsync(entityId, cancellationToken))
        {
            return await base.CanEditDocumentsAsync(entityId, directoryId, cancellationToken);
        }

        return false;
    }
}