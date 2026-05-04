using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Interfaces;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories;
using QueryKit.Repositories.Enums;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using System.Security.Authentication;

namespace MyPortal.Data.Repositories.Base;

public class EntityRepository<TEntity> : BaseEntityRepository<TEntity, Guid>, IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
    protected IAuthorizationService AuthorizationService { get; }

    public EntityRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(factory)
    {
        AuthorizationService = authorizationService;
    }

    private readonly SortOptions _defaultSort = new()
    {
        Criteria = new[] { new SortCriterion { ColumnName = "Id", Direction = SortDirection.Ascending } }
    };

    protected override Task<PageResult<T>> GetListPagedAsync<T>(string sql, object? parameters, FilterOptions? filter, SortOptions? sort, PageOptions? paging,
        bool includeDeleted = false, CancellationToken cancellationToken = new(), IDbTransaction? transaction = null)
    {
        if (paging != null && sort == null)
        {
            sort = _defaultSort;
        }

        return base.GetListPagedAsync<T>(sql, parameters, filter, sort, paging, includeDeleted, cancellationToken,
            transaction);
    }

    public override Task<PageResult<TEntity>> GetListPagedAsync(FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        bool includeDeleted = false, CancellationToken cancellationToken = default, IDbTransaction? transaction = null)
    {
        if (paging != null && sort == null)
        {
            sort = _defaultSort;
        }

        return base.GetListPagedAsync(filter, sort, paging, includeDeleted, cancellationToken, transaction);
    }

    public override Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default,
        IDbTransaction? transaction = null)
    {
        if (entity is IAuditableEntity auditable)
        {
            var userId = AuthorizationService.GetCurrentUserId();

            if (!userId.HasValue)
            {
                throw new AuthenticationException("Not authenticated.");
            }

            var ipAddress = AuthorizationService.GetCurrentUserIpAddress() ?? "";

            auditable.CreatedAt = DateTime.UtcNow;
            auditable.CreatedById = userId.Value;
            auditable.CreatedByIpAddress = ipAddress;
            auditable.LastModifiedByIpAddress = ipAddress;
            auditable.LastModifiedAt = DateTime.UtcNow;
            auditable.LastModifiedById = userId.Value;
        }

        if (entity is IVersionedEntity { Version: 0 } versionedEntity)
        {
            versionedEntity.Version = 1;
        }

        return base.InsertAsync(entity, cancellationToken, transaction);
    }

    public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default,
        IDbTransaction? transaction = null)
    {
        // For versioned entities, UpdateWithVersionAsync's row-version guard is the atomicity
        // guarantee — no transaction needed around the read+write. For non-versioned entities
        // the only thing in the read-then-write window is the IsSystem check, and IsSystem is a
        // near-immutable seed-time property; the race is theoretical. Callers needing stronger
        // atomicity should pass an IDbTransaction.
        var entityInDb = await GetByIdAsync(entity.Id, cancellationToken, transaction);

        if (entityInDb is null)
        {
            throw new NotFoundException($"{typeof(TEntity).Name} not found.");
        }

        if (entityInDb is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot modify a system entity.");
        }

        if (entity is IAuditableEntity auditable)
        {
            var userId = AuthorizationService.GetCurrentUserId();

            if (!userId.HasValue)
            {
                throw new AuthenticationException("Not authenticated.");
            }

            var ipAddress = AuthorizationService.GetCurrentUserIpAddress() ?? "";

            auditable.LastModifiedById = userId.Value;
            auditable.LastModifiedAt = DateTime.UtcNow;
            auditable.LastModifiedByIpAddress = ipAddress;
        }

        if (entity is IVersionedEntity versioned)
        {
            var expected = versioned.Version;

            // Will throw ConcurrencyException if 0 rows (version mismatch or deleted)
            await base.UpdateWithVersionAsync(entity, expected, cancellationToken, transaction);

            versioned.Version = expected + 1;

            return entity;
        }

        return await base.UpdateAsync(entity, cancellationToken, transaction);
    }

    public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default,
        bool softDelete = true, IDbTransaction? transaction = null)
    {
        var entity = await GetByIdAsync(id, cancellationToken, transaction);

        if (entity is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot delete a system entity.");
        }

        return await base.DeleteAsync(id, cancellationToken, softDelete, transaction);
    }
}
