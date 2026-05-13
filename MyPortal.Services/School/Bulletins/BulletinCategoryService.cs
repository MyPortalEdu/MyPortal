using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.School;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.School.Bulletins;

public class BulletinCategoryService : IBulletinCategoryService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBulletinCategoryRepository _repository;
    private readonly ILogger<BulletinCategoryService> _logger;

    public BulletinCategoryService(IAuthorizationService authorizationService,
        IBulletinCategoryRepository repository, ILogger<BulletinCategoryService> logger)
    {
        _authorizationService = authorizationService;
        _repository = repository;
        _logger = logger;
    }

    public async Task<IList<BulletinCategoryResponse>> GetAllAsync(bool includeInactive,
        CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);

        // Categories are a small set (typically <20 rows). Pulling the lot and filtering /
        // ordering in memory keeps the call site free of QueryKit's filter/sort plumbing.
        var page = await _repository.GetListPagedAsync(cancellationToken: cancellationToken);

        return page.Items
            .Where(c => includeInactive || c.Active)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<BulletinCategoryResponse> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);

        var entity = await _repository.GetByIdAsync(categoryId, cancellationToken)
                     ?? throw new NotFoundException("Bulletin category not found.");

        return Map(entity);
    }

    public async Task<Guid> CreateAsync(BulletinCategoryUpsertRequest model, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        var entity = new BulletinCategory
        {
            Id = SqlConvention.SequentialGuid(),
            Name = model.Name,
            Icon = model.Icon,
            ColourCode = model.ColourCode,
            DisplayOrder = model.DisplayOrder,
            Active = model.Active,
            IsSystem = false
        };

        await _repository.InsertAsync(entity, cancellationToken);

        _logger.LogInformation("Bulletin category created: {categoryId}", entity.Id);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid categoryId, BulletinCategoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        var entity = await _repository.GetByIdAsync(categoryId, cancellationToken)
                     ?? throw new NotFoundException("Bulletin category not found.");

        entity.Name = model.Name;
        entity.Icon = model.Icon;
        entity.ColourCode = model.ColourCode;
        entity.DisplayOrder = model.DisplayOrder;
        entity.Active = model.Active;
        entity.Version = model.ExpectedVersion;

        await _repository.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Bulletin category updated: {categoryId}", categoryId);
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        await _authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        await _repository.DeleteAsync(categoryId, cancellationToken);

        _logger.LogInformation("Bulletin category deleted: {categoryId}", categoryId);
    }

    private static BulletinCategoryResponse Map(BulletinCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Icon = c.Icon,
        ColourCode = c.ColourCode,
        DisplayOrder = c.DisplayOrder,
        Active = c.Active,
        IsSystem = c.IsSystem,
        Version = c.Version
    };
}
