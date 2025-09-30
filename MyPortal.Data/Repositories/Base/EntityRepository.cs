using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories.Base;

public class EntityRepository<TEntity> : BaseEntityRepository<TEntity, Guid>, IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
    protected EntityRepository(IConnectionFactory factory) : base(factory)
    {
    }
}