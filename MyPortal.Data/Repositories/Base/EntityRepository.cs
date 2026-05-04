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
using System.Transactions;
using IsolationLevel = System.Transactions.IsolationLevel;

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
        // The system-entity check and the actual UPDATE are two separate Dapper calls.
        // Wrap them in an ambient transaction so they enlist on the same connection /
        // commit together — and so they enlist into any outer scope (e.g. a service
        // that's already inside CreateTransactionScope).
        using var tx = CreateTransactionScope();

        var entityInDb = await GetByIdAsync(entity.Id, cancellationToken);

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

        TEntity result;
        if (entity is IVersionedEntity versioned)
        {
            var expected = versioned.Version;

            // Will throw ConcurrencyException if 0 rows (version mismatch or deleted)
            // This is most likely a conflict since we've already had to retrieve the original entity by this point
            await base.UpdateWithVersionAsync(entity, expected, cancellationToken);

            versioned.Version = expected + 1;

            result = entity;
        }
        else
        {
            result = await base.UpdateAsync(entity, cancellationToken, transaction);
        }

        tx.Complete();
        return result;
    }

    public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default,
        bool softDelete = true, IDbTransaction? transaction = null)
    {
        using var tx = CreateTransactionScope();
        
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot delete a system entity.");
        }

        var result = await base.DeleteAsync(id, cancellationToken, softDelete, transaction);

        tx.Complete();
        return result;
    }

    private static TransactionScope CreateTransactionScope() =>
        new(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
}