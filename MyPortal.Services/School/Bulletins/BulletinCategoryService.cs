using FluentValidation;
using FluentValidation.Results;
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

public class BulletinCategoryService(
    IAuthorizationService authorizationService,
    IBulletinCategoryRepository repository,
    ILogger<BulletinCategoryService> logger)
    : IBulletinCategoryService
{
    public async Task<IList<BulletinCategoryResponse>> GetAllAsync(bool includeInactive,
        CancellationToken cancellationToken)
    {
        await authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);

        // Categories are a small set (typically <20 rows). Pulling the lot and filtering /
        // ordering in memory keeps the call site free of QueryKit's filter/sort plumbing.
        var page = await repository.GetListPagedAsync(cancellationToken: cancellationToken);

        return page.Items
            .Where(c => includeInactive || c.Active)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<BulletinCategoryResponse> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        await authorizationService.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);

        var entity = await repository.GetByIdAsync(categoryId, cancellationToken)
                     ?? throw new NotFoundException("Bulletin category not found.");

        return Map(entity);
    }

    public async Task<Guid> CreateAsync(BulletinCategoryUpsertRequest model, CancellationToken cancellationToken)
    {
        await authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        await EnsureNameUniqueAsync(model.Name, excludeCategoryId: null, cancellationToken);

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

        await repository.InsertAsync(entity, cancellationToken);

        logger.LogInformation("Bulletin category created: {categoryId}", entity.Id);
        return entity.Id;
    }

    public async Task UpdateAsync(Guid categoryId, BulletinCategoryUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        var entity = await repository.GetByIdAsync(categoryId, cancellationToken)
                     ?? throw new NotFoundException("Bulletin category not found.");

        await EnsureNameUniqueAsync(model.Name, excludeCategoryId: categoryId, cancellationToken);

        entity.Name = model.Name;
        entity.Icon = model.Icon;
        entity.ColourCode = model.ColourCode;
        entity.DisplayOrder = model.DisplayOrder;
        entity.Active = model.Active;
        entity.Version = model.ExpectedVersion;

        await repository.UpdateAsync(entity, cancellationToken);

        logger.LogInformation("Bulletin category updated: {categoryId}", categoryId);
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        await authorizationService.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, cancellationToken);

        await repository.DeleteAsync(categoryId, cancellationToken);

        logger.LogInformation("Bulletin category deleted: {categoryId}", categoryId);
    }

    // Category names must be unique (the DB enforces UQ_BulletinCategories_Name; this guard turns the
    // clash into a friendly error instead of a 500). The set is tiny, so an in-memory check is fine.
    private async Task EnsureNameUniqueAsync(string name, Guid? excludeCategoryId, CancellationToken cancellationToken)
    {
        var page = await repository.GetListPagedAsync(cancellationToken: cancellationToken);

        if (page.Items.Any(c => c.Id != excludeCategoryId
                                && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException(
                [new ValidationFailure("Name", $"A bulletin category named '{name}' already exists.")]);
        }
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
