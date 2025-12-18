using MyPortal.Common.Interfaces;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;
using QueryKit.Repositories.Enums;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories.Base;

public class EntityReadRepository<TEntity> : BaseEntityReadRepository<TEntity, Guid>, IEntityReadRepository<TEntity>
    where TEntity : class, IEntity
{
    public EntityReadRepository(IDbConnectionFactory factory) : base(factory)
    {
    }

    private SortOptions DefaultSort = new SortOptions
    {
        Criteria = new[] { new SortCriterion { ColumnName = "Id", Direction = SortDirection.Ascending } }
    };

    protected override Task<PageResult<T>> GetListPagedAsync<T>(string sql, object? parameters, FilterOptions? filter, SortOptions? sort, PageOptions? paging,
        bool includeDeleted = false, CancellationToken cancellationToken = new CancellationToken())
    {
        if (paging != null && sort == null)
        {
            sort = DefaultSort;
        }

        return base.GetListPagedAsync<T>(sql, parameters, filter, sort, paging, includeDeleted, cancellationToken);
    }

    public override Task<PageResult<TEntity>> GetListPagedAsync(FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        bool includeDeleted = false, CancellationToken cancellationToken = new CancellationToken())
    {
        if (paging != null && sort == null)
        {
            sort = DefaultSort;
        }

        return base.GetListPagedAsync(filter, sort, paging, includeDeleted, cancellationToken);
    }
}